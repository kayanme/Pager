using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Pager;
using Pager.Classes;
using static Pager.PageManagerConfiguration;

namespace Benchmark.Pager
{
    public class RecordAddBenchmark
    {
        IPageManager _manager;
        [Params(PageSize.Kb4,PageSize.Kb8)]        
        public PageSize PageSize;

        [Params(WriteMethod.Naive, WriteMethod.FixedSize, WriteMethod.VariableSize)]
        public WriteMethod WriteMethod;

        private FixedRecordTypedPage<TestRecord> _page;
        private ComplexRecordTypePage<TestRecord> _page2;
        private FileStream _other;
        [GlobalSetup]
        public void Init()
        {
            var config = new PageManagerConfiguration { SizeOfPage = PageSize };
            var pconfig = new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordType = new FixedSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            };
            var vconfig = new VariableRecordTypePageConfiguration<TestRecord>
            {
                RecordMap = new Dictionary<byte, VariableSizeRecordDeclaration<TestRecord>> { { 1,
                          new VariableSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, _=>7) } }
                
            };
            config.PageMap.Add(1, pconfig);
            config.PageMap.Add(2, vconfig);
            _manager = new PageManagerFactory().CreateManager("testFile", config,true);
            _page = _manager.CreatePage(1) as FixedRecordTypedPage<TestRecord>;
            _page2 = _manager.CreatePage(2) as ComplexRecordTypePage<TestRecord>;
            _other = File.Open("testfile2" , FileMode.OpenOrCreate);
        }

        [Benchmark]
        public void AddRecord()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize:  _page.AddRecord(new TestRecord { Values = new byte[] { 1, 2, 3, 4, 5, 6, 7 } });break;
                case WriteMethod.VariableSize: _page.AddRecord(new TestRecord { Values = new byte[] { 1, 2, 3, 4, 5, 6, 7 } }); break;
                case WriteMethod.Naive: _other.Write(new byte[] { 1, 1, 2, 3, 4, 5, 6, 7, }, 0, 8); break;
            }
        }

       [Benchmark]
        public void AddRecordWithFlush()
        {
            switch (WriteMethod)
            {
                case WriteMethod.FixedSize: _page.AddRecord(new TestRecord { Values = new byte[] { 1, 2, 3, 4, 5, 6, 7 } }); _page.Flush(); break;
                case WriteMethod.VariableSize: _page2.AddRecord(new TestRecord { Values = new byte[] { 1, 2, 3, 4, 5, 6, 7 } }); _page2.Flush(); break;
                case WriteMethod.Naive: _other.Write(new byte[] { 1, 1, 2, 3, 4, 5, 6, 7, }, 0, 8); _other.Flush(); break;
            }
            _page.AddRecord(new TestRecord { Values = new byte[] { 1, 2, 3, 4, 5, 6, 7 } });
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
                File.Delete("testFile");
                File.Delete("testFile2");
            }
            catch
            {

            }
            Thread.Sleep(100);
        }
    }
}
