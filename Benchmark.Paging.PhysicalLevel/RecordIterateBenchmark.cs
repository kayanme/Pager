using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace Benchmark.Paging.PhysicalLevel
{
    public class Physical_RecordIterateBenchmark
    {
        public static PageManagerConfiguration.PageSize PageSize = PageManagerConfiguration.PageSize.Kb8;
        public static int SizeInKb = PageSize == PageManagerConfiguration.PageSize.Kb4 ? 4 : 8;

        private FileStream _other;
        private IPageManager _manager;

        public static int PageCount = 200;
       
        private int _count;
       
        private class Config : PageManagerConfiguration
        {
            public Config(PageSize size) : base(size)
            {
                DefinePageType(1)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 7);

                //DefinePageType(2)
                //    .AsPageWithRecordType<TestRecord>()
                //    .WithMultipleTypeRecord(_ => 1)
                //    .UsingRecordDefinition(1, (t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); },
                //        _ => 7)
                //    .UsingRecordDefinition(2, (t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); },
                //        _ => 7);

                DefinePageType(3)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 7)
                    .ApplyLogicalSortIndex();
            }
        }


        [Params(WriteMethod.FixedSize)]
        public WriteMethod WriteMethod;

        [GlobalSetup]
        public void Init()
        {
            _count = 0;
            var config = new Config(PageSize);
            _manager = new PageManagerFactory().CreateManager("testFile", config, true);
            if (WriteMethod == WriteMethod.FixedSize)
            {
                var pages = Enumerable.Range(0, PageCount).Select(k => _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(1))).ToArray();

                foreach (var page in pages)
                {
                    while (page.AddRecord(new TestRecord())!=null) ;
                    page.Flush();
                }
                
            }
            //else if (WriteMethod == WriteMethod.VariableSize)
            //{
            //    var pages2 = Enumerable.Range(0, PageCount).Select(k => _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(2))).ToArray();
            //    foreach (var page in pages2)
            //    {
            //        while (page.AddRecord(new TestRecord())) ;
            //        page.Flush();
            //    }
            //}
            else if (WriteMethod == WriteMethod.FixedSizeWithOrder)
            {
                var pages2 = Enumerable.Range(0, PageCount).Select(k => _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(3))).ToArray();
                foreach (var page in pages2)
                {
                    while (page.AddRecord(new TestRecord())!=null) ;
                    page.Flush();
                }
            }
            _other = System.IO.File.Open("testfile2", FileMode.OpenOrCreate);
            _other.SetLength(SizeInKb * PageCount);
        }

        [Benchmark]
        public object IteratePage()
        {

            using (var p = _manager.GetRecordAccessor<TestRecord>(new PageReference(_count)))
            {
                _count = (_count + 1) % PageCount;
                return p.IterateRecords().ToArray();
            }          
        }
        [GlobalCleanup]
        public void DeleteFile()
        {
            _other.Dispose();
         
            _manager.Dispose();
            Thread.Sleep(100);
            try
            {
                System.IO.File.Delete("testFile");
                System.IO.File.Delete("testFile2");
            }
            catch
            {

            }
            Thread.Sleep(100);
        }
    }
}
