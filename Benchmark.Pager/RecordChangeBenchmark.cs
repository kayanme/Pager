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
    public class RecordChangeBenchmark
    {
        IPageManager _manager;
        
        public static PageSize PageSize = PageSize.Kb8;
        public static int SizeInKb = PageSize == PageSize.Kb4?4:8;
        //private FixedRecordTypedPage<TestRecord>[] _pages;       
        //private ComplexRecordTypePage<TestRecord>[] _pages2;
        private FileStream _other;

        private static List<Tuple<int,byte>> _changes;
        public static int PageCount = 200;
        static RecordChangeBenchmark()
        {
            var rnd = new Random();
            _changes = Enumerable.Range(0, 20000).Select(k => Tuple.Create(rnd.Next(PageCount* SizeInKb * 1024), (byte)rnd.Next(255))).ToList();
        }
        private int _count;

        [Params(WriteMethod.VariableSize, WriteMethod.Naive)]
        public WriteMethod WriteMethod;
        [GlobalSetup]
        public void Init()
        {
            _count = 0;
            var config = new PageManagerConfiguration { SizeOfPage = PageSize };
            var pconfig = new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordType = new FixedSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            };
            var vconfig = new VariableRecordTypePageConfiguration<TestRecord>
            {
                RecordMap = new Dictionary<byte, VariableSizeRecordDeclaration<TestRecord>> {
                    { 1, new VariableSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, _=>7) },
                    { 2, new VariableSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, _=>7) } }

            };
            config.PageMap.Add(1, pconfig);
            config.PageMap.Add(2, vconfig);
            _manager = new PageManagerFactory().CreateManager("testFile", config,true);
            if (WriteMethod == WriteMethod.FixedSize)
            {
                var _pages = Enumerable.Range(0, PageCount).Select(k => _manager.CreatePage(1) as FixedRecordTypedPage<TestRecord>).ToArray();

                foreach (var page in _pages)
                {
                    while (page.AddRecord(new TestRecord())) ;
                    page.Flush();
                }
            }
            else if (WriteMethod == WriteMethod.VariableSize)
            {
                var _pages2 = Enumerable.Range(0, PageCount).Select(k => _manager.CreatePage(2) as ComplexRecordTypePage<TestRecord>).ToArray();
                foreach (var page in _pages2)
                {
                    while (page.AddRecord(new TestRecord())) ;
                    page.Flush();
                }
            }
            _other = File.Open("testfile2" , FileMode.OpenOrCreate);
            _other.SetLength(SizeInKb * PageCount);


        }

        private TypedPage PageWrite(bool flush)
        {
            var change = _changes[_count];
            var page = _manager.RetrievePage(new PageReference(change.Item1 / SizeInKb/1024));
            var shift = change.Item1 % (SizeInKb*1024);
            if (WriteMethod == WriteMethod.FixedSize)
            {
                var t = page as FixedRecordTypedPage<TestRecord>;
                var record = t.GetRecord(new PageRecordReference { Record = shift / 8, Page = page.Reference });
                record.Values[shift % 7] = change.Item2;
                t.StoreRecord(record);
                _count += _count & _changes.Count;
            }
            else
            {
              
                    var t = page as ComplexRecordTypePage<TestRecord>;
                    var record = t.GetRecord(new PageRecordReference { Record = shift / 8, Page = page.Reference });
                    record.Values[shift % 7] = change.Item2;
                    t.StoreRecord(record);
                    _count += _count & _changes.Count;
                
            }
                if (flush)
                    page.Flush();
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
                case WriteMethod.FixedSize: _manager.GroupFlush(Enumerable.Range(0, 50).Select(k => PageWrite(false)).ToArray()); break;
                case WriteMethod.VariableSize: _manager.GroupFlush(Enumerable.Range(0, 50).Select(k => PageWrite(false)).ToArray()); break;
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
            var change = _changes[_count];
            _other.Position = change.Item1 - (change.Item1 % 7);
            byte[] data = new byte[7];
            _other.Read(data, 0, 7);
            data[change.Item1 % 7] = change.Item2;
            _other.Position = change.Item1 - (change.Item1 % 7);
            _other.Write(data, 0, 7);
            _count += _count & _changes.Count;
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
            
        }
    }
}
