using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Pager;
using static Pager.PageMapConfiguration;

namespace Benchmark.Pager
{
    public class RecordAddBenchmark
    {
        IPageManager _manager;
        [Params(PageSize.Kb4,PageSize.Kb8)]
        
        public PageSize PageSize;
        private FixedRecordTypedPage<TestRecord> _page;
        private FileStream _other;
        [GlobalSetup]
        public void Init()
        {
            var config = new PageMapConfiguration { SizeOfPage = PageSize };
            config.PageMap.Add(1, typeof(TestRecord));
            _manager = new PageManagerFactory().CreateManager("testFile", config,true);
            _page = _manager.CreatePage<TestRecord>();
            _other = File.Open("testfile2" , FileMode.OpenOrCreate);
        }

        [Benchmark]
        public void AddRecord()
        {
            _page.AddRecord(new TestRecord { Values = new byte[] { 1, 2, 3, 4, 5, 6, 7 } });
        }

       [Benchmark]
        public void AddRecordWithFlush()
        {
            _page.AddRecord(new TestRecord { Values = new byte[] { 1, 2, 3, 4, 5, 6, 7 } });
            _page.Flush();
        }

        [Benchmark]
        public void NaiveWrite()
        {
            _other.Write(new byte[] { 1, 1, 2, 3, 4, 5, 6, 7, }, 0, 8);
        }

        [Benchmark]
        public void NaiveWriteWithFlush()
        {
            _other.Write(new byte[] { 1, 1, 2, 3, 4, 5, 6, 7, }, 0, 8);
            _other.Flush();
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
