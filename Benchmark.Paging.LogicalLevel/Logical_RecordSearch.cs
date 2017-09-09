using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes;
using FIle.Paging.LogicalLevel.Classes.Configurations;
using FIle.Paging.LogicalLevel.Contracts;

namespace Benchmark.Paging.LogicalLevel
{
    public class Logical_RecordSearch
    {

        IPageManager _manager;
        [Params(PageManagerConfiguration.PageSize.Kb4, PageManagerConfiguration.PageSize.Kb8)]
        public PageManagerConfiguration.PageSize PageSize;

        private class Config : LogicalPageManagerConfiguration
        {

            public Config(PageManagerConfiguration.PageSize pageSize) : base(pageSize)
            {
                DefinePageType(1)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 4)
                    .ApplyLogicalSortIndex()
                    .ApplyRecordOrdering((a) => a.Order);

                DefinePageType(2)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 4);

                DefinePageType(3).AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 4)
                    .AsVirtualHeapPage(4);
            }
        }


        private IPage<TestRecord>[] _pages;

        private int[] _data;
        private int _count;
        private readonly Random _rnd = new Random();
        [GlobalSetup]
        public void Init()
        {
            var config = new Config(PageSize);

            _manager = new LogicalPageManagerFactory().CreateManager("RecordSearch", config, true);
            _pages = new IPage<TestRecord>[5];
            _pages[0] = _manager.GetRecordAccessor<TestRecord>(_manager.CreatePage(1));
          
            while (_pages[0].AddRecord(new TestRecord { Order = _rnd.Next(100) }) != null) ;

            _data = Enumerable.Range(0, 1000).Select(_ => _rnd.Next(100)).ToArray();
        }

        [Benchmark]
        public void Search()
        {
            var d = _data[_count];
            var e = _pages[0] as IOrderedPage<TestRecord, int>;
            var r = e.FindByKey(d);
            _count = (_count + 1) % _data.Length;
        }
    }
}
