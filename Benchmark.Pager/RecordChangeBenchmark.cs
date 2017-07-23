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
    public class RecordChangeBenchmark
    {
        IPageManager _manager;
        [Params(PageSize.Kb4,PageSize.Kb8)]        
        public PageSize PageSize;
        private FixedRecordTypedPage<TestRecord> _page;
        private FileStream _other;

        private static List<Tuple<int,byte>> _changes;

        static RecordChangeBenchmark()
        {
            var rnd = new Random();
            _changes = Enumerable.Range(0, 2000).Select(k => Tuple.Create(rnd.Next(4096), (byte)rnd.Next(255))).ToList();
        }
        private int _count;

        [GlobalSetup]
        public void Init()
        {
            _count = 0;
            var config = new PageMapConfiguration { SizeOfPage = PageSize };
            config.PageMap.Add(1, typeof(TestRecord));
            _manager = new PageManagerFactory().CreateManager("testFile"+Guid.NewGuid(), config,true);
            _page = _manager.CreatePage<TestRecord>();
            while (_page.AddRecord(new TestRecord())) ;
            _page.Flush();
            _other = File.Open("testfile2" , FileMode.OpenOrCreate);
            _other.SetLength(PageSize == PageSize.Kb4 ? 4096 : 8192);


        }

        [Benchmark]
        public void AddRecord()
        {
            var change = _changes[_count];
            var record = _page.GetRecord(new PageRecordReference { Record = change.Item1 / 8, Page = new PageReference(0) });
            record.Values[change.Item1 % 7] = change.Item2;
            _page.StoreRecord(record);
            _count+=_count & _changes.Count;
        }

       [Benchmark]
        public void AddRecordWithFlush()
        {
            var change = _changes[_count];
            var record = _page.GetRecord(new PageRecordReference { Record = change.Item1 / 8, Page = new PageReference(0) });
            record.Values[change.Item1 % 7] = change.Item2;
            _page.StoreRecord(record);
            _page.Flush();
            _count += _count & _changes.Count;
        }

        [Benchmark]
        public void AddRecordGroupWithFlush()
        {
            for (int i = 0; i <= 50; i++)
            {
                var change = _changes[_count];
                var record = _page.GetRecord(new PageRecordReference { Record = change.Item1 / 8, Page = new PageReference(0) });
                record.Values[change.Item1 % 7] = change.Item2;
                _page.StoreRecord(record);
                _count += _count & _changes.Count;
            }
            _page.Flush();
        }


        [Benchmark]
        public void NaiveWriteGroupWithFlush()
        {
            for (int i = 0; i <= 50; i++)
            {
                var change = _changes[_count];
                _other.Position = change.Item1 / 7 * 7;
                byte[] data = new byte[7];
                _other.Read(data, 0, 7);
                data[change.Item1 % 7] = change.Item2;
                _other.Write(data, 0, 7);
            }
            _other.Flush();
        }

        [Benchmark]
        public void NaiveWrite()
        {
            var change = _changes[_count];
            _other.Position = change.Item1 -  (change.Item1 % 7);
            byte[] data = new byte[7];
            _other.Read(data, 0, 7);
            data[change.Item1 % 7] = change.Item2;
            _other.Position = change.Item1 - (change.Item1 % 7);
            _other.Write(data, 0, 7);
            _count += _count & _changes.Count;
        }

        [Benchmark]
        public void NaiveWriteWithFlush()
        {
            var change = _changes[_count];
            _other.Position = change.Item1 / 7 * 7;
            byte[] data = new byte[7];
            _other.Read(data, 0, 7);
            data[change.Item1 % 7] = change.Item2;
            _other.Write(data,0, 7);
            _other.Flush();
        }

        [GlobalCleanup]
        public void DeleteFile()
        {
            _other.Dispose();
            _page.Dispose();
            _manager.Dispose();
            
            try
            {
                File.Delete("testFile");
                File.Delete("testFile2");
            }
            catch
            {

            }
            
        }
    }
}
