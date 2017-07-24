using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Pager;
using static Pager.PageManagerConfiguration;

namespace Benchmark.Pager
{
    public class RecordChangeBenchmark
    {
        IPageManager _manager;
        
        public static PageSize PageSize = PageSize.Kb8;
        public static int SizeInKb = PageSize == PageSize.Kb4?4:8;
        private FixedRecordTypedPage<TestRecord>[] _pages;
        private FileStream _other;

        private static List<Tuple<int,byte>> _changes;
        public static int PageCount = 200;
        static RecordChangeBenchmark()
        {
            var rnd = new Random();
            _changes = Enumerable.Range(0, 20000).Select(k => Tuple.Create(rnd.Next(PageCount* SizeInKb * 1024), (byte)rnd.Next(255))).ToList();
        }
        private int _count;

        [GlobalSetup]
        public void Init()
        {
            _count = 0;
            var config = new PageManagerConfiguration { SizeOfPage = PageSize };
            config.PageMap.Add(1, new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordType = new RecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            });
            _manager = new PageManagerFactory().CreateManager("testFile", config,true);
            _pages = Enumerable.Range(0,PageCount).Select(k=> _manager.CreatePage<TestRecord>()).ToArray();
            foreach (var page in _pages)
            {
                while (page.AddRecord(new TestRecord())) ;
                page.Flush();
            }
            _other = File.Open("testfile2" , FileMode.OpenOrCreate);
            _other.SetLength(SizeInKb * PageCount);


        }

        private TypedPage PageWrite(bool flush)
        {
            var change = _changes[_count];
            var page = _manager.RetrievePage<TestRecord>(new PageReference(change.Item1 / SizeInKb/1024));
            var shift = change.Item1 % (SizeInKb*1024);
            var record = page.GetRecord(new PageRecordReference { Record = shift / 8, Page = page.Reference });
            record.Values[shift % 7] = change.Item2;
            page.StoreRecord(record);
            _count += _count & _changes.Count;
            if (flush)
                page.Flush();
            return page;
        }

        [Benchmark]
        public void AddRecord()
        {
            PageWrite(false);
        }

       [Benchmark]
        public void AddRecordWithFlush()
        {
            PageWrite(false);
        }

        [Benchmark]
        public void AddRecordGroupWithFlush()
        {
        _manager.GroupFlush(Enumerable.Range(0, 50).Select(k => PageWrite(false)).ToArray());
           
        }

        private void NaiveAdd()
        {
            var change = _changes[_count];
            _other.Position = change.Item1 - (change.Item1 % 7);
            byte[] data = new byte[7];
            _other.Read(data, 0, 7);
            data[change.Item1 % 7] = change.Item2;
            _other.Position = change.Item1 - (change.Item1 % 7);
            _other.Write(data, 0, 7);
            _count += _count & _changes.Count;
        }

        [Benchmark]
        public void NaiveWrite()
        {
            NaiveAdd();
        }

        [Benchmark]
        public void NaiveWriteWithFlush()
        {
            NaiveAdd();
            _other.Flush();
        }


        [Benchmark]
        public void NaiveWriteGroupWithFlush()
        {
            for (int i = 0; i <= 50; i++)
            {
                NaiveAdd();
            }
            _other.Flush();
        }

        [GlobalCleanup]
        public void DeleteFile()
        {
            _other.Dispose();
            foreach (var page in _pages)
                page.Dispose();
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
