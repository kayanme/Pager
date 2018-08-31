using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace Test.Integration.Physical
{
    [TestClass]
    public class Tests
    {
        private IPageManager _pageManager;

        [TestInitialize]
        public void Init()
        {
            if (System.IO.File.Exists("testFile"))
                System.IO.File.Delete("testFile");
            using (var factory = new PageManagerFactory())
            {
                _pageManager = factory.CreateManager("testFile", new PageConfiguration(), true);
            }
        }

        [TestCleanup]
        public void Clean()
        {
            _pageManager.Dispose();
            System.IO.File.Delete("testFile");
        }

        [TestMethod]
        public void WorkingWithHeaderedPage()
        {
            var sw = Stopwatch.StartNew();
            var pageRef = _pageManager.CreatePage(2);
            var headerPage = _pageManager.GetHeaderAccessor<TestHeader>(pageRef);
            var page = _pageManager.GetRecordAccessor<TestRecord>(pageRef);
            var pageInfo = _pageManager.GetPageInfo(pageRef);
            var list = new List<TypedRecord<TestRecord>>();
            var header = new TestHeader {HeaderInfo = "ABCDEFG_123456789_abcdefyui_!!?:"};
            headerPage.ModifyHeader(header);

            for (long i = 0; pageInfo.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                list.Add(page.AddRecord(r));
                
            }
            var h = headerPage.GetHeader();
            Assert.AreEqual(header.HeaderInfo,h.HeaderInfo);
            foreach (var testRecord in list)
            {
                var y = page.GetRecord(testRecord.Reference);
                Assert.AreEqual(testRecord.Data.Value, y.Data.Value);
            }
            foreach (var y in page.IterateRecords())
            {
               
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);
                y.Data.Value = -y.Data.Value;
                e.Data.Value = -e.Data.Value;
                page.StoreRecord(e);
            }
            h = headerPage.GetHeader();
            Assert.AreEqual(h.HeaderInfo, header.HeaderInfo);
        }

        [TestMethod]
        public void CreatingLargeFile()
        {
            var sw = Stopwatch.StartNew();
            for(long i = 0; i < 1024*1024/2; i++)
            {
                var rf = _pageManager.CreatePage(1);
               var page = _pageManager.GetRecordAccessor<TestRecord>(rf);
                page.AddRecord(new TestRecord());
                page.Dispose();
                (_pageManager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(rf);
            }
            sw.Stop();
            Debug.Print(sw.Elapsed.ToString("g"));
        }

        [TestMethod]
        public void WorkingWithPage()
        {
            var sw = Stopwatch.StartNew();            
            var pageRef = _pageManager.CreatePage(1);           
            var page = _pageManager.GetRecordAccessor<TestRecord>(pageRef);
            var pageInfo = _pageManager.GetPageInfo(pageRef);
            var list = new List<TypedRecord<TestRecord>>();
            for (long i = 0; pageInfo.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                list.Add(page.AddRecord(r));
               
            }
            foreach (var testRecord in list)
            {
                var y = page.GetRecord(testRecord.Reference);
                Assert.AreEqual(testRecord.Data.Value,y.Data.Value);
            }
            foreach (var y in page.IterateRecords())
            {
             
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);
                y.Data.Value = -y.Data.Value;
                e.Data.Value = -e.Data.Value;
                page.StoreRecord(e);
            }

            foreach (var y in page.IterateRecords().Take(20))
            {
              
                var e = list.First(k => k.Data.Value == y.Data.Value);
                page.FreeRecord(e);
                list.Remove(e);
            }

            foreach (var y in page.IterateRecords())
            {
              
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);             
            }

            for (long i = 1000000; pageInfo.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                list.Add(page.AddRecord(r));
                
            }

            foreach (var y in page.IterateRecords())
            {
              
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }
            (_pageManager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(pageRef);
            page.Dispose();
            page = _pageManager.GetRecordAccessor<TestRecord>(pageRef);
            foreach (var y in page.IterateRecords())
            {
              
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }
            _pageManager.DeletePage(pageRef, false); 
            page.Dispose();
            sw.Stop();
            Debug.Print(sw.Elapsed.ToString("g"));
        }


        [TestMethod]
        public void WorkingWithOrderedPage()
        {
            var sw = Stopwatch.StartNew();
            
            var pageRef = _pageManager.CreatePage(3);
            var page = _pageManager.GetRecordAccessor<TestRecord>(pageRef);
            var pageInfo = _pageManager.GetPageInfo(pageRef);
            var list = new List<TypedRecord<TestRecord>>();
            for (long i = 0; pageInfo.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                list.Add(page.AddRecord(r));
               
            }

            foreach (var testRecord in list)
            {
                var y = page.GetRecord(testRecord.Reference);
                Assert.AreEqual(testRecord.Data.Value, y.Data.Value);
            }
            foreach (var y in page.IterateRecords())
            {
              
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);
                y.Data.Value = -y.Data.Value;
                e.Data.Value = -e.Data.Value;
                page.StoreRecord(e);
            }

            _pageManager.GetSorter<TestRecord> (pageRef).ApplyOrder(list.OrderBy(k=>k.Data.Value).Select(k=>k.Reference).ToArray());

            foreach (var record in page.IterateRecords().Zip(list.OrderBy(k => k.Data.Value),
                (pg,ls)=>new{pg ,ls}))
            {           
                Assert.AreEqual(record.pg.Data.Value, record.ls.Data.Value);                
            }

            foreach (var y in page.IterateRecords().Take(20))
            {
            
                var e = list.First(k => k.Data.Value == y.Data.Value);
                page.FreeRecord(e);
                list.Remove(e);
            }

            foreach (var y in page.IterateRecords())
            {
              
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }

            foreach (var record in list)
            {
                var y = page.GetRecord(record.Reference);              
                Assert.AreEqual(record.Reference, y.Reference);
            }

            for (long i = 1000000; pageInfo.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                list.Add(page.AddRecord(r));
               
            }

            foreach (var y in page.IterateRecords())
            {
              
                var e = list.First(k => k.Data.Value == y.Data.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }
            _pageManager.DeletePage(pageRef, false);
            sw.Stop();
            Debug.Print(sw.Elapsed.ToString("g"));
        }

        [TestMethod]
        public async Task WorkingWithRecordLocks()
        {
            var sw = Stopwatch.StartNew();

            var pageRef = _pageManager.CreatePage(4);
            var bag = new List<PageRecordReference>();
            const int recCount = 10;
            using (var acc = _pageManager.GetRecordAccessor<TestRecord>(pageRef))
            {
                for (int i = 0; i < recCount; i++)
                {
                    var rec = acc.AddRecord(new TestRecord(0));
                    bag.Add(rec.Reference);
                }
            }
            var valArr = new long[recCount];
            var rnd = new Random();
            var log = new ConcurrentQueue<ValueTuple<int, string>>();
            async Task ProcessorLine(int taskNum)
            {
                var acc = _pageManager.GetRecordAccessor<TestRecord>(pageRef);
                var locker = _pageManager.GetPageLocks(pageRef);
                for (int i = 0; i < 1E4; i++)
                {
                    await ProcessorPart(taskNum,acc, locker);
                }
            }

            async Task ProcessorPart(int taskNum,IPage<TestRecord> acc,IPhysicalLocks locker)
            {
                var action = rnd.Next(0, 5);
                var num = rnd.Next(bag.Count);
                var recRef = bag[num];

                switch (action)
                {
                    //читаем без ожидания
                    case 0:
                        log.Enqueue((taskNum, $"{taskNum} acquiring shared for {num}"));
                        if (locker.AcqureLock(recRef, 0, out var lckToken))
                        {
                            log.Enqueue((taskNum, $"{taskNum} acqured shared for {num} ({lckToken.SharedLockCount})"));
                            acc.GetRecord(recRef);
                            locker.ReleaseLock(lckToken);
                            log.Enqueue((taskNum, $"{taskNum} released shared for {num}"));
                        }
                        break;
                    //читаем с ожиданием
                    case 1:
                        log.Enqueue((taskNum, $"{taskNum} waiting shared for {num}"));
                        lckToken = await locker.WaitLock(recRef, 0);
                        log.Enqueue((taskNum, $"{taskNum} acqured shared for {num}  ({lckToken.SharedLockCount})"));
                        var r = acc.GetRecord(recRef);
                        locker.ReleaseLock(lckToken);
                        log.Enqueue((taskNum, $"{taskNum} released shared for {num}"));
                        break;
                    //пишем без ожидания
                    case 2:
                        log.Enqueue((taskNum, $"{taskNum} acquiring nonshared for {num}"));
                        if (locker.AcqureLock(recRef, 1, out lckToken))
                        {
                            log.Enqueue((taskNum, $"{taskNum} acqured nonshared for {num}"));
                            r = acc.GetRecord(recRef);
                            r.Data.Value = r.Data.Value + 1;
                            acc.StoreRecord(r);
                            locker.ReleaseLock(lckToken);
                            log.Enqueue((taskNum, $"{taskNum} released nonshared for {num}"));
                            Interlocked.Increment(ref valArr[num]);
                        }
                        break;
                    //пишем с ожиданием
                    case 3:
                        log.Enqueue((taskNum, $"{taskNum} waiting nonshared for {num}"));
                        lckToken = await locker.WaitLock(recRef, 1);
                        log.Enqueue((taskNum, $"{taskNum} acquired nonshared for {num}"));
                        r = acc.GetRecord(recRef);
                        r.Data.Value = r.Data.Value + 1;
                        acc.StoreRecord(r);
                        locker.ReleaseLock(lckToken);
                        log.Enqueue((taskNum, $"{taskNum} released nonshared for {num}"));
                        Interlocked.Increment(ref valArr[num]);
                        break;
                    //пишем с эскалацией
                    case 4:
                        log.Enqueue((taskNum, $"{taskNum} waiting shared for {num}"));
                        lckToken = await locker.WaitLock(recRef, 0);
                        log.Enqueue((taskNum, $"{taskNum} acquired shared for {num}"));                        
                        log.Enqueue((taskNum, $"{taskNum} waiting escalation for {num}"));
                        lckToken = await locker.WaitForLockLevelChange(lckToken, 1);
                        log.Enqueue((taskNum, $"{taskNum} acquired nonshared for {num}"));
                        r = acc.GetRecord(recRef);
                        r.Data.Value = r.Data.Value + 1;
                        acc.StoreRecord(r);
                        locker.ReleaseLock(lckToken);
                        log.Enqueue((taskNum, $"{taskNum} released nonshared for {num}"));
                        Interlocked.Increment(ref valArr[num]);
                        break;
                }
            }

            await Task.WhenAll(Enumerable.Range(0, 30).Select(k => ProcessorLine(k)).ToArray());

            long[] recordValues;
            using (var acc = _pageManager.GetRecordAccessor<TestRecord>(pageRef))
            {
                recordValues = bag.Select(acc.GetRecord).Select(k => k.Data.Value).ToArray();
             }
            var res = recordValues.Zip(valArr, (a, b)=>$"{a}, {b}");
            Assert.IsTrue(recordValues.Zip(valArr, (a,b)=>a==b).All(k=>k));
            sw.Stop();
            Debug.Print(sw.Elapsed.ToString("g"));
        }
    }
}
