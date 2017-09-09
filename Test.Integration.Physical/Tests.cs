using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using File.Paging.PhysicalLevel.Classes;
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
    }
}
