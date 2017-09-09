using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace Benchmark.Paging.PhysicalLevel
{
    public class Physical_RecordSearchBenchmark
    {
        private IPageManager _manager;
        private class Config : PageManagerConfiguration
        {
            public Config(PageSize size) : base(size)
            {
                DefinePageType(1)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 7);


                DefinePageType(2)
                    .AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition((ref TestRecord t, byte[] b) => { t.FillByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillFromByteArray(b); }, 7)
                    .ApplyLogicalSortIndex();
            }
        }
        [Params(WriteMethod.FixedSize,WriteMethod.FixedSizeWithOrder)]
        public WriteMethod WriteMethod;

        [Params(PageManagerConfiguration.PageSize.Kb4,PageManagerConfiguration.PageSize.Kb8)]
        public PageManagerConfiguration.PageSize PageSize;

        private uint[] _data;

        [GlobalSetup]
        public void Init()
        {
           uint count = 1;
            var config = new Config(PageSize);
            _manager = new PageManagerFactory().CreateManager("testFile", config, true);

            _page = _manager.CreatePage(1);
            using (var a = _manager.GetRecordAccessor<TestRecord>(_page))
            {
                while (a.AddRecord(new TestRecord {IntValue = count++}) !=
                       null) ;
                (_manager as IPhysicalPageManipulation).Flush(_page);
                var r = a.IterateRecords().First().Data;
            }
            count = 0;
            _page2 = _manager.CreatePage(2);
            using (var a = _manager.GetRecordAccessor<TestRecord>(_page2))
                while (a.AddRecord(new TestRecord { IntValue = count++ }) !=
                   null) ;

            var rnd = new Random();
            _data = Enumerable.Range(0, 4000)
                .Select(_ => (uint)rnd.Next(0,
                    Math.Min(_manager.GetPageInfo(_page).UsedRecords, _manager.GetPageInfo(_page2).UsedRecords)))
                    .ToArray();

        }

        private PageReference _page;
        private PageReference _page2;
        private int _cutCount = 0;

        [Benchmark]
        public void ScanSearch()
        {
            var pg = WriteMethod == WriteMethod.FixedSize ? _page : _page2;
            var d = _data[_cutCount];
            using (var p = _manager.GetRecordAccessor<TestRecord>(pg))
            {
                foreach (var rec in p.IterateRecords())
                {
                    if (rec.Data.IntValue == d)
                        break;
                }
            }
            _cutCount = (_cutCount + 1) % _data.Length;
        }

        [Benchmark]
        public void BinarySearch()
        {
            var pg = WriteMethod == WriteMethod.FixedSize ? _page : _page2;
            var d = _data[_cutCount];
            using (var p = _manager.GetBinarySearchForPage<TestRecord>(pg))
            {
                while (true)
                {
                    var c = p.Current;
                    if (c == null)
                        break;
                    if (c.Data.IntValue == d)
                        break;
                    if (c.Data.IntValue > d)
                        if (!p.MoveLeft()) break;
                    if (c.Data.IntValue < d)
                        if (!p.MoveRight()) break;
                }
            }
            _cutCount = (_cutCount + 1) % _data.Length;
        }
    }
}
