using System.IO;
using System.Threading;
using BenchmarkDotNet.Attributes;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel;
using System;
using System.Linq;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace Benchmark.Paging.PhysicalLevel
{
    public class Physical_RecordAddBenchmark
    {
        IPageManager _manager;
        [Params(PageManagerConfiguration.PageSize.Kb4,PageManagerConfiguration.PageSize.Kb8)]        
        public PageManagerConfiguration.PageSize PageSize;

        [Params(8,16,32,64,128)]
        public int ExtentRate;

        [Params(WriteMethod.Naive, WriteMethod.FixedSize,WriteMethod.FixedSizeWithOrder,WriteMethod.Image,WriteMethod.VariableSize)]
        public WriteMethod WriteMethod;

        private unsafe class PageConfig : PageManagerConfiguration
        {

            private  void Copy(byte[] b, ref TestRecord2 t)
            {
                if (t.Data == null)
                    t.Data = new byte[_size];
                fixed (byte* ft = b)
                fixed (byte* ft2 = t.Data)
                {
                    Buffer.MemoryCopy(ft, ft2, _size,_size);
                }
            }
            private int _size;
            public PageConfig(PageSize size,int extentSize) : base(size,extentSize)
            {
                _size = SizeOfPage == PageSize.Kb4 ? 4096 : 8192;
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

                DefinePageType(4)
                   .AsPageWithRecordType<TestRecord2>()
                   .AsPlainImage((ref TestRecord2 t, byte[] b) => { t.Data = b; }, Copy);
            }
        }

        private IPage<TestRecord> _page;
        private IPage<TestRecord> _page2;
        private IPage<TestRecord> _page3;
        private IPage<TestRecord2> _page4;
        private FileStream _other;
        [GlobalSetup]
        public void Init()
        {
            System.IO.File.Delete("testFile");
            var config = new PageConfig( PageSize,ExtentRate);
           
            _manager = new PageManagerFactory().CreateManagerWithAutoFileCreation("testFile", config);
            _page =_manager.GetRecordAccessor<TestRecord>( _manager.CreatePage(1));
            _page2 = _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(1));
            _page3 = _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(3));
            _page4 = _manager.GetRecordAccessor<TestRecord2>(_manager.CreatePage(4));
            _other = System.IO.File.Open("testfile2" , FileMode.OpenOrCreate);
            
        }

        [Benchmark]
        public void AddRecord()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize:  _page.AddRecord(new TestRecord( new byte[] { 1, 2, 3, 4, 5, 6, 7 } ));break;
                case WriteMethod.Image:
                    var rec = _page4.IterateRecords().First();
                    rec.Data.Data = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
                    _page4.StoreRecord(rec);
                    break;
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
                case WriteMethod.Image:
                    var rec = _page4.IterateRecords().First();
                    rec.Data.Data = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
                    _page4.StoreRecord(rec);
                    _page4.Flush();
                    break;
                case WriteMethod.FixedSizeWithOrder: _page3.AddRecord(new TestRecord(new byte[] { 1, 2, 3, 4, 5, 6, 7 } )); _page.Flush(); break;
                case WriteMethod.VariableSize: _page2.AddRecord(new TestRecord(new byte[] { 1, 2, 3, 4, 5, 6, 7 } )); _page2.Flush(); break;
                case WriteMethod.Naive: _other.Write(new byte[] { 1, 1, 2, 3, 4, 5, 6, 7, }, 0, 8); _other.Flush(); break;
            }
            _page.AddRecord(new TestRecord (new byte[] { 1, 2, 3, 4, 5, 6, 7 } ));
            _page.Flush();
        }


        [Benchmark]
        public void BufferedPageTaking()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize:
                    _page.Dispose();
                    _page = _manager.GetRecordAccessor<TestRecord>(new PageReference(0));
                    break;
                case WriteMethod.Image:
                    _page4.Dispose();
                    _page4 = _manager.GetRecordAccessor<TestRecord2>(new PageReference(3));                  
                    break;
                case WriteMethod.FixedSizeWithOrder:
                    _page3.Dispose();
                    _page3 = _manager.GetRecordAccessor<TestRecord>(new PageReference(2));
                    break;
                case WriteMethod.VariableSize:
                    _page2.Dispose();
                    _page2 = _manager.GetRecordAccessor<TestRecord>(new PageReference(1));
                    break;
                case WriteMethod.Naive: return;
            }           
        }


        [Benchmark]
        public void NonBufferedPageTaking()
        {
            
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize:
                    (_manager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(new PageReference(0));
                    _page.Dispose();
                    _page = _manager.GetRecordAccessor<TestRecord>(new PageReference(0));
                    break;
                case WriteMethod.Image:
                    (_manager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(new PageReference(3));
                    _page4.Dispose();
                    _page4 = _manager.GetRecordAccessor<TestRecord2>(new PageReference(3));
                    break;
                case WriteMethod.FixedSizeWithOrder:
                    (_manager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(new PageReference(2));
                    _page3.Dispose();
                    _page3 = _manager.GetRecordAccessor<TestRecord>(new PageReference(2));
                    break;
                case WriteMethod.VariableSize:
                    (_manager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(new PageReference(1));
                    _page2.Dispose();
                    _page2 = _manager.GetRecordAccessor<TestRecord>(new PageReference(1));
                    break;
                case WriteMethod.Naive: return;
            }
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
