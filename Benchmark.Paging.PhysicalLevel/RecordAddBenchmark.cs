using System.Collections.Generic;
using System.IO;
using System.Threading;
using BenchmarkDotNet.Attributes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace Benchmark.Paging.PhysicalLevel
{
    public class Physical_RecordAddBenchmark
    {
        IPageManager _manager;
        [Params(PageManagerConfiguration.PageSize.Kb4,PageManagerConfiguration.PageSize.Kb8)]        
        public PageManagerConfiguration.PageSize PageSize;

        [Params(WriteMethod.Naive, WriteMethod.FixedSize)]
        public WriteMethod WriteMethod;

        private class PageConfig : PageManagerConfiguration
        {
            public PageConfig(PageManagerConfiguration.PageSize size) : base(size)
            {
                DefinePageType(1)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 7);

             

                DefinePageType(2)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, _ => 7);

                DefinePageType(3)
                    .AsPageWithRecordType<TestRecord>()                    
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 7)
                    .ApplyLogicalSortIndex();
            }
        }

        private IPage<TestRecord> _page;
        private IPage<TestRecord> _page2;
        private IPage<TestRecord> _page3;
        private FileStream _other;
        [GlobalSetup]
        public void Init()
        {
            System.IO.File.Delete("testFile");
            var config = new PageConfig( PageSize);
           
            _manager = new PageManagerFactory().CreateManager("testFile", config,true);
            _page =_manager.GetRecordAccessor<TestRecord>( _manager.CreatePage(1));
            _page2 = _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(1));
            _page3 = _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(3));
            _other = System.IO.File.Open("testfile2" , FileMode.OpenOrCreate);
        }

        [Benchmark]
        public void AddRecord()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize:  _page.AddRecord(new TestRecord( new byte[] { 1, 2, 3, 4, 5, 6, 7 } ));break;
                case WriteMethod.FixedSizeWithOrder: _page3.AddRecord(new TestRecord (new byte[] { 1, 2, 3, 4, 5, 6, 7 } )); break;
                case WriteMethod.VariableSize: _page2.AddRecord(new TestRecord (new byte[] { 1, 2, 3, 4, 5, 6, 7 } )); break;
                case WriteMethod.Naive: _other.Write(new byte[] { 1, 1, 2, 3, 4, 5, 6, 7, }, 0, 8); break;
            }
        }

       [Benchmark]
        public void AddRecordWithFlush()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize: _page.AddRecord(new TestRecord(new byte[] { 1, 2, 3, 4, 5, 6, 7 } )); _page.Flush(); break;
                case WriteMethod.FixedSizeWithOrder: _page3.AddRecord(new TestRecord(new byte[] { 1, 2, 3, 4, 5, 6, 7 } )); _page.Flush(); break;
                case WriteMethod.VariableSize: _page2.AddRecord(new TestRecord(new byte[] { 1, 2, 3, 4, 5, 6, 7 } )); _page2.Flush(); break;
                case WriteMethod.Naive: _other.Write(new byte[] { 1, 1, 2, 3, 4, 5, 6, 7, }, 0, 8); _other.Flush(); break;
            }
            _page.AddRecord(new TestRecord (new byte[] { 1, 2, 3, 4, 5, 6, 7 } ));
            _page.Flush();
        }       

        [GlobalCleanup]
        public void DeleteFile()
        {
            _other.Dispose();
            _page.Dispose();
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
