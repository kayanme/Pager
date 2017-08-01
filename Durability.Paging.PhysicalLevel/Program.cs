using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;
using Pager.Classes;

namespace Durability.Paging.PhysicalLevel
{
    class Program
    {
        private static IPageManager _pageManager;
        private static IPage<TestRecord> _lastunempty;
        private static SharedDataStore _store;
        static void Main(string[] args)
        {
            if (File.Exists("teststress"))
               File.Delete("teststress");
            var config = new PageManagerConfiguration { SizeOfPage = PageManagerConfiguration.PageSize.Kb8 };
            config.PageMap = new Dictionary<byte, PageConfiguration>
            { {2,new FixedRecordTypePageConfiguration<TestRecord> {
                RecordMap = new FixedSizeRecordDeclaration<TestRecord>((r,b)=>Buffer.BlockCopy(r.Data.ToByteArray(),0,b,0,b.Count()),(b,r)=>r.Data =new Guid(b.ToArray()),16) } },
             {1,new VariableRecordTypePageConfiguration<TestRecord>(_=>1) {
                RecordMap = new Dictionary<byte, VariableSizeRecordDeclaration<TestRecord>>
                         { { 1, new VariableSizeRecordDeclaration<TestRecord>((r,b)=>Buffer.BlockCopy(r.Data.ToByteArray(),0,b,0,b.Count()),(b,r)=>r.Data =new Guid(b.ToArray()),_=>16) }
                     } } } };
            var f = new PageManagerFactory();
            _pageManager = f.CreateManager("teststress", config, true);
            _lastunempty = _pageManager.CreatePage(1) as IPage<TestRecord>;
            _store = new SharedDataStore();
            var d = new CompositeDisposable(CreateWorker(),
            CreateWorker(), CreateWorker());
            Console.ReadKey();
            _stop = true;
            Console.ReadKey();
         
            d.Dispose();
            _pageManager.Dispose();
            File.Delete("teststress");
            GC.Collect();

            Console.ReadKey();
            _store = null;
            GC.Collect();
            Console.ReadKey();
        }
        private static volatile bool _stop;
        private static IDisposable CreateWorker()
        {
            var rnd = new Random();
            var queue = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(.1)).Where(k=> !_stop).Select(_ => rnd.Next(3)).ObserveOn(NewThreadScheduler.Default).Publish();
          
            //      var queue = new[] { 0, 1 }.ToObservable().ObserveOn(ImmediateScheduler.Instance);
            var d = new CompositeDisposable(
                queue.Where(k => k == 0).Subscribe(_ => ProcessAdd()),
                queue.Where(k => k == 1).Subscribe(_ => ProcessUpdate()),
                queue.Where(k => k == 2).Subscribe(_ => ProcessDelete()),
                Observable.FromEvent<string>(h => _log += h, h => _log -= h).ObserveOn(new EventLoopScheduler()).Subscribe(Console.WriteLine));
            queue.Connect();
            
            return d;
        }

        private static Action<string> _log = _ => { };

        private static void ProcessAdd()
        {
            TestRecord rec = new TestRecord { Data = Guid.NewGuid() };
            _log($"Adding {rec.Data}");
            while (!_lastunempty.AddRecord(rec))
                _lastunempty = _pageManager.CreatePage(1) as IPage<TestRecord>;
            _log($"Added {rec.Data}");
            _store.Add(rec.Reference, rec);
        }

        private static void ProcessUpdate()
        {
            var rec = _store.SelectRandom();
            if (rec == null)
                return;
            var newData = Guid.NewGuid();
            var old = new TestRecord { Data = rec.Data };
            _log($"{old.Data} becoming {newData}");
            if (rec == null)
                return;
            var page = _pageManager.RetrievePage(rec.Reference.Page) as IPage<TestRecord>;
            var record = page.GetRecord(rec.Reference);
            if (record == null)
                _log($"{rec.Data} found deleted");
            else
            {
                record.Data = newData;
                page.StoreRecord(record);
                Console.WriteLine($"{old.Data} became {record.Data}");
                _store.Update(rec.Reference, old, record);
            }
        }

        private static void ProcessDelete()
        {
            var rec = _store.SelectRandom();
            if (rec == null)
                return;
            _log($"deleting {rec.Data}");
            if (rec == null)
                return;
            var page = _pageManager.RetrievePage(rec.Reference.Page) as IPage<TestRecord>;
            var old = rec.Reference.Copy();
            var t = page.GetRecord(old);
            page.FreeRecord(t);
            _log($"deleted {rec.Data}");
            _store.Delete(rec.Reference, rec);
        }
    }
}
