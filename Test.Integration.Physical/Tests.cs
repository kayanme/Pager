using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using File.Paging.PhysicalLevel.Classes.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

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
        public void WorkingWithPage()
        {
            var sw = Stopwatch.StartNew();
            var page = _pageManager.CreatePage(1) as IPage<TestRecord>;
            var list = new List<TestRecord>();
            for (long i = 0; page.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                page.AddRecord(r);
                list.Add(r);
            }

            foreach (var testRecord in list)
            {
                var y = page.GetRecord(testRecord.Reference);
                Assert.AreEqual(testRecord.Value,y.Value);
            }
            foreach (var record in page.IterateRecords())
            {
                var y = page.GetRecord(record);
                var e = list.First(k => k.Value == y.Value);
                Assert.AreEqual(e.Reference, y.Reference);
                y.Value = -y.Value;
                e.Value = -e.Value;
                page.StoreRecord(e);
            }

            foreach (var reference in page.IterateRecords().Take(20))
            {
                var y = page.GetRecord(reference);
                var e = list.First(k => k.Value == y.Value);
                page.FreeRecord(e);
                list.Remove(e);
            }

            foreach (var record in page.IterateRecords())
            {
                var y = page.GetRecord(record);
                var e = list.First(k => k.Value == y.Value);
                Assert.AreEqual(e.Reference, y.Reference);             
            }

            for (long i = 1000000; page.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                page.AddRecord(r);
                list.Add(r);
            }

            foreach (var record in page.IterateRecords())
            {
                var y = page.GetRecord(record);
                var e = list.First(k => k.Value == y.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }
            (_pageManager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(page.Reference);
            page.Dispose();
            page = _pageManager.RetrievePage(page.Reference) as IPage<TestRecord>;
            foreach (var record in page.IterateRecords())
            {
                var y = page.GetRecord(record);
                var e = list.First(k => k.Value == y.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }
            _pageManager.DeletePage(page.Reference,false); 
            page.Dispose();
            sw.Stop();
            Debug.Print(sw.Elapsed.ToString("g"));
        }


        [TestMethod]
        public void WorkingWithOrderedPage()
        {
            var sw = Stopwatch.StartNew();
            var page = _pageManager.CreatePage(3) as IPage<TestRecord>;
            var list = new List<TestRecord>();
            for (long i = 0; page.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                page.AddRecord(r);
                list.Add(r);
            }

            foreach (var testRecord in list)
            {
                var y = page.GetRecord(testRecord.Reference);
                Assert.AreEqual(testRecord.Value, y.Value);
            }
            foreach (var record in page.IterateRecords())
            {
                var y = page.GetRecord(record);
                var e = list.First(k => k.Value == y.Value);
                Assert.AreEqual(e.Reference, y.Reference);
                y.Value = -y.Value;
                e.Value = -e.Value;
                page.StoreRecord(e);
            }

            (page as ILogicalRecordOrderManipulation).ApplyOrder(list.OrderBy(k=>k.Value).Select(k=>k.Reference).ToArray());

            foreach (var record in page.IterateRecords().Zip(list.OrderBy(k => k.Value),(pg,ls)=>new{pg = page.GetRecord(pg),ls}))
            {           
                Assert.AreEqual(record.pg.Value, record.ls.Value);                
            }

            foreach (var reference in page.IterateRecords().Take(20))
            {
                var y = page.GetRecord(reference);
                var e = list.First(k => k.Value == y.Value);
                page.FreeRecord(e);
                list.Remove(e);
            }

            foreach (var record in page.IterateRecords())
            {
                var y = page.GetRecord(record);
                var e = list.First(k => k.Value == y.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }

            foreach (var record in list)
            {
                var y = page.GetRecord(record.Reference);              
                Assert.AreEqual(record.Reference, y.Reference);
            }

            for (long i = 1000000; page.PageFullness < .9; i++)
            {
                var r = new TestRecord(i);
                page.AddRecord(r);
                list.Add(r);
            }

            foreach (var record in page.IterateRecords())
            {
                var y = page.GetRecord(record);
                var e = list.First(k => k.Value == y.Value);
                Assert.AreEqual(e.Reference, y.Reference);
            }
            _pageManager.DeletePage(page.Reference, false);
            sw.Stop();
            Debug.Print(sw.Elapsed.ToString("g"));
        }
    }
}
