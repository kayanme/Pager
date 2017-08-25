using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace Benchmark.Paging.PhysicalLevel
{
    public class Physical_RecordChangeBenchmark
    {
        IPageManager _manager;
        
        public static PageManagerConfiguration.PageSize PageSize = PageManagerConfiguration.PageSize.Kb8;
        public static int SizeInKb = PageSize == PageManagerConfiguration.PageSize.Kb4?4:8;
      
        private FileStream _other;

        private static readonly List<Tuple<int,byte>> Changes;
        public static int PageCount = 200;
        static Physical_RecordChangeBenchmark()
        {
            var rnd = new Random();
            Changes = Enumerable.Range(0, 20000).Select(k => Tuple.Create(rnd.Next(PageCount* SizeInKb * 1024), (byte)rnd.Next(255))).ToList();
        }
        private int _count;

        private class Config:PageManagerConfiguration
        {
            public Config(PageSize size):base(size)
            {
                DefinePageType(1)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7);

                DefinePageType(2)
                    .AsPageWithRecordType<TestRecord>()
                    .WithMultipleTypeRecord(_ => 1)
                    .UsingRecordDefinition(1, (t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); },
                        _ => 7)
                    .UsingRecordDefinition(2, (t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); },
                        _ => 7);
            }
        }
        [Params(WriteMethod.VariableSize, WriteMethod.Naive)]
        public WriteMethod WriteMethod;
        [GlobalSetup]
        public void Init()
        {
            _count = 0;
            var config = new Config(PageSize);                   
            _manager = new PageManagerFactory().CreateManager("testFile", config,true);
            if (WriteMethod == WriteMethod.FixedSize)
            {
                var pages = Enumerable.Range(0, PageCount).Select(k => _manager.CreatePage(1) as FixedRecordTypedPage<TestRecord>).ToArray();

                foreach (var page in pages)
                {
                    while (page.AddRecord(new TestRecord())) ;
                    page.Flush();
                }
            }
            else if (WriteMethod == WriteMethod.VariableSize)
            {
                var pages2 = Enumerable.Range(0, PageCount).Select(k => _manager.CreatePage(2) as ComplexRecordTypePage<TestRecord>).ToArray();
                foreach (var page in pages2)
                {
                    while (page.AddRecord(new TestRecord())) ;
                    page.Flush();
                }
            }
            _other = System.IO.File.Open("testfile2" , FileMode.OpenOrCreate);
            _other.SetLength(SizeInKb * PageCount);


        }

        private IPage PageWrite(bool flush)
        {
            var change = Changes[_count];
            var page = _manager.RetrievePage(new PageReference(change.Item1 / SizeInKb / 1024));
            var shift = change.Item1 % (SizeInKb * 1024);
            if (WriteMethod == WriteMethod.FixedSize)
            {
                var t = page as FixedRecordTypedPage<TestRecord>;
                var record = t.GetRecord(new PhysicalPositionPersistentPageRecordReference(page.Reference,(ushort)shift));
                record.Values[shift % 7] = change.Item2;
                t.StoreRecord(record);
                _count += _count & Changes.Count;
            }
            else
            {

                var t = page as ComplexRecordTypePage<TestRecord>;
                var record = t.GetRecord(new PhysicalPositionPersistentPageRecordReference(page.Reference, (ushort)shift));
                if (record != null)
                {
                    record.Values[shift % 7] = change.Item2;
                    t.StoreRecord(record);
                    _count += _count & Changes.Count;
                }

            }
            if (flush)
                (page as IPhysicalRecordManipulation).Flush();
            return page;

        }

        [Benchmark]
        public void AddRecord()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize: PageWrite(false);  break;
                case WriteMethod.VariableSize: PageWrite(false); break;
                case WriteMethod.Naive: NaiveAdd();  break;
            }
         
        }

       [Benchmark]
        public void AddRecordWithFlush()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize: PageWrite(true); break;
                case WriteMethod.VariableSize: PageWrite(true); break;
                case WriteMethod.Naive: NaiveAdd();_other.Flush(); break;
            }
        }

        [Benchmark]
        public void AddRecordGroupWithFlush()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize: (_manager as IPhysicalPageManipulation).GroupFlush(Enumerable.Range(0, 50).Select(k => PageWrite(false)).ToArray()); break;
                case WriteMethod.VariableSize: (_manager as IPhysicalPageManipulation).GroupFlush(Enumerable.Range(0, 50).Select(k => PageWrite(false)).ToArray()); break;
                case WriteMethod.Naive:
                    for (int i = 0; i <= 50; i++)
                    {
                        NaiveAdd();
                    }
                    _other.Flush();  break;
            }          
        }

        private void NaiveAdd()
        {
            var change = Changes[_count];
            _other.Position = change.Item1 - (change.Item1 % 7);
            byte[] data = new byte[7];
            _other.Read(data, 0, 7);
            data[change.Item1 % 7] = change.Item2;
            _other.Position = change.Item1 - (change.Item1 % 7);
            _other.Write(data, 0, 7);
            _count += _count & Changes.Count;
        }

     
      
        [GlobalCleanup]
        public void DeleteFile()
        {
            _other.Dispose();           
            _manager.Dispose();
            
            try
            {
                System.IO.File.Delete("testFile");
                System.IO.File.Delete("testFile2");
            }
            catch
            {

            }
            
        }
    }
}
