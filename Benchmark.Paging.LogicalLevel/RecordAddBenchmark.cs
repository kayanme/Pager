using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Benchmark.Paging.LogicalLevel;
using BenchmarkDotNet.Attributes;
using FIle.Paging.LogicalLevel.Classes;
using FIle.Paging.LogicalLevel.Classes.Configurations;
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

       

        private IPage<TestRecord>[] _pages;
        
        private FileStream _other;
        private Random _rnd = new Random();
        [GlobalSetup]
        public void Init()
        {
            var config = new LogicalPageManagerConfiguration { SizeOfPage = PageSize };
            var pconfig = new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordMap = new FixedSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 4)
            };
           
            var orderConfig = new OrderedLogicalPageConfiguration<TestRecord, int> { KeySelector = (a) => a.Order };
            config.Configuration.Add(1, orderConfig);            
            config.PageMap.Add(1, pconfig);
            config.PageMap.Add(2, pconfig);
            _manager = new LogicalPageManagerFactory().CreateManager("testFile", config,true);
            _pages = new IPage<TestRecord>[4];
            _pages[0] = _manager.CreatePage(1) as IPage<TestRecord>;
            _pages[1] = _manager.CreatePage(2) as IPage<TestRecord>;
            _pages[2] = _manager.CreatePage(1) as IPage<TestRecord>;
            _pages[3] = _manager.CreatePage(2) as IPage<TestRecord>;
            while (_pages[2].AddRecord(new TestRecord { Order = _rnd.Next(1000) })) ;
            while (_pages[3].AddRecord(new TestRecord { Order = _rnd.Next(1000) })) ;
            _other = File.Open("testfile2" , FileMode.OpenOrCreate);

        }

        //[Benchmark]
        public void AddRecordWithOrder()
        {
            var t = _rnd.Next(1000);
            var t2 = new TestRecord { Order = t };
            _pages[0].AddRecord(t2);
            
        }

       //[Benchmark(Baseline = true)]
        public void AddRecordWithoutOrder()
        {
            var t = _rnd.Next(1000);
            var t2 = new TestRecord { Order = t };
            _pages[1].AddRecord(t2);
            

        }

        [Benchmark]
        public void ChangeRecordWithOrder()
        {
            var t = _rnd.Next(1000);
            var t2 = _pages[2].IterateRecords().First();
            t2.Order = t;
            _pages[2].StoreRecord(t2);
            
        }

        [Benchmark(Baseline = true)]
        public void ChangeRecordWithoutOrder()
        {
            var t = _rnd.Next(1000);
            var t2 = _pages[3].IterateRecords().First();
            t2.Order = t;
            _pages[3].StoreRecord(t2);

        }

        [GlobalCleanup]
        public void DeleteFile()
        {
            _other.Dispose();
             
            _manager.Dispose();
            
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
