using BenchmarkDotNet.Attributes;
using FIle.Paging.LogicalLevel.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimeArchiver.Classes.Paging;
using TimeArchiver.Contracts;

namespace Benchmark.TimeArchiver
{
    public class TimeArchiver_PagesBenchmark
    {
        [Params(500)]
        public int PagesPresentForEachTag;

        [Params(500)]
        public int TagCount;

        private IDataSearch _searcher;
        [GlobalSetup]
        public void Setup()
        {
                     var pmf = new LogicalPageManagerFactory();
            _searcher = new DataSearch("roots", "index1", "index2", "data", pmf);
            foreach (var tag in Enumerable.Range(0, TagCount))
                _searcher.CreateTag(tag, TagType.Int).Wait();
            long maxStamp = 0;
            async Task InsertBlock(long tag,int shift)
            {
                var vals = Enumerable.Range(0 + shift, DataPageRecord<int>.MaxRecordsOnPage)
               .Select(k => new DataRecord<int> { Data = k, VersionStamp = k, Stamp = k }).ToArray();
                maxStamp = Math.Max(maxStamp, vals.Max(k => k.Stamp));
                await _searcher.InsertBlock(tag, vals);
            }
            foreach(var tag in Enumerable.Range(0,TagCount))
            foreach (var t in Enumerable.Range(0, PagesPresentForEachTag).Select(k => k * DataPageRecord<int>.MaxRecordsOnPage))
            {
                InsertBlock(tag,t).Wait();
            }
           
            var rnd = new Random();
            var shifts = Enumerable.Range(0, 1000).Select(_ => rnd.Next(300000));
            foreach (var shift in shifts)
            {
                var vals = Enumerable.Range(0 + shift, DataPageRecord<int>.MaxRecordsOnPage)
                      .Select(k => new DataRecord<int> { Data = k, VersionStamp = k, Stamp = k }).ToArray();
                _blocks.Add(vals);
            }
            foreach (var shift in shifts)
            {
                _reads.Add((rnd.Next(0, (int)maxStamp / 2), rnd.Next((int)maxStamp / 2, (int)maxStamp)));
            }

        }

        private List<(int, int)> _reads = new List<(int, int)>();
        private List<DataRecord<int>[]> _blocks = new List<DataRecord<int>[]>();
        private int _count = 0;

        [Benchmark]
        public void AddPage()
        {
            var vals = _blocks[_count];
            _searcher.InsertBlock(_count % TagCount, vals).Wait();
            _count = (_count + 1) % 1000;
        }

        [Benchmark]
        public void ReadPage()
        {
            var vals = _reads[_count];
            _searcher.FindInRangeInt(_count % TagCount, vals.Item1,vals.Item2);
            _count = (_count + 1) % 1000;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _searcher.Dispose();
            System.IO.File.Delete("roots");
            System.IO.File.Delete("index1");
            System.IO.File.Delete("index2");
            System.IO.File.Delete("data");
        }
    }
}
