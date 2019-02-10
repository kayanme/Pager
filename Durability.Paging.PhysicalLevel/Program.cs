using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel;
using System.Threading.Tasks;

namespace Durability.Paging.PhysicalLevel
{
    class Program
    {
        private static IPageManager _pageManager;
        private static IPage<TestRecord> _lastunempty;
        private static SharedDataStore _store;
        static void Main(string[] args)
        {
            if (System.IO.File.Exists("teststress"))
                System.IO.File.Delete("teststress");
            var config = new PageConfig(PageManagerConfiguration.PageSize.Kb8);
          
            var f = new PageManagerFactory();
            _pageManager = f.CreateManagerWithAutoFileCreation("teststress", config);
            _lastunempty =_pageManager.GetRecordAccessor<TestRecord>( _pageManager.CreatePage(2));
            _store = new SharedDataStore();
            var d = new CompositeDisposable(CreateWorker(),
            CreateWorker(), CreateWorker());
            Task.Delay(20000).Wait();
            _stop = true;
         //   Console.ReadKey();
         
            d.Dispose();
            _pageManager.Dispose();
            System.IO.File.Delete("teststress");
            GC.Collect();

            //Console.ReadKey();
            _store = null;
            GC.Collect();
           // Console.ReadKey();
        }
        private static volatile bool _stop;
        private static IDisposable CreateWorker()
        {
            var rnd = new Random();
            var queue = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(1)).Where(k=> !_stop).Select(_ => rnd.Next(3)).ObserveOn(NewThreadScheduler.Default).Publish();
          
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
            TypedRecord<TestRecord> trec;
            while ((trec = _lastunempty.AddRecord(rec))==null)
                _lastunempty = _pageManager.GetRecordAccessor<TestRecord>( _pageManager.CreatePage(2));
            _log($"Added {rec.Data}");
            _store.Add(trec.Reference, rec);
        }

        private static void ProcessUpdate()
        {
            var rec = _store.SelectRandom();
            if (rec == null || rec.Item2.Data == default(Guid))
                return;
            var newData = Guid.NewGuid();
            var old = new TestRecord { Data = rec.Item2.Data };
            _log($"{old.Data} becoming {newData}");
            if (rec.Item2.Data == default(Guid))
                return;
            var page = _pageManager.GetRecordAccessor<TestRecord>(rec.Item1.Page);
            var record = page.GetRecord(rec.Item1);
            if (record == null)
                _log($"{rec.Item2.Data} found deleted");
            else
            {
                record.Data.Data = newData;
                page.StoreRecord(record);
                Console.WriteLine($"{old.Data} became {record.Data}");
                _store.Update(rec.Item1, old, record.Data);
            }
        }

        private static void ProcessDelete()
        {
            var rec = _store.SelectRandom();
            if (rec == null)
                return;
            _log($"deleting {rec.Item2.Data}");
            if (rec == null)
                return;
            var page = _pageManager.GetRecordAccessor<TestRecord>(rec.Item1.Page);
            var old = rec.Item1.Copy();
            var t = page.GetRecord(old);
            if (t!=null)
            page.FreeRecord(t);
            _log($"deleted {rec.Item2.Data}");
            _store.Delete(rec.Item1, rec.Item2);
        }
    }
}
