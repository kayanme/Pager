using System;
using System.Collections.Generic;
using System.Linq;
using FIle.Paging.LogicalLevel.Classes;
using FIle.Paging.LogicalLevel.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pager;
using Pager.Classes;
using Rhino.Mocks;

namespace Test.Paging.LogicalLevel
{
    [TestClass]
    public class OrderedPage
    {
        public TestContext TestContext { get; set; }

        private IOrderedPage<TestRecord> Create(params TestRecord[] initialState)
        {
            var physPage = new MockRepository().StrictMock<IPage<TestRecord>>();

            physPage.Expect(k => k.IterateRecords()).Return(initialState.AsEnumerable());
            physPage.Replay();
            var page = new OrderedPage<TestRecord, int>(physPage, k => k.Order);
            physPage.BackToRecord();
            physPage.Expect(k => k.Reference).Repeat.Any().Return(new PageReference(0));

            TestContext.Properties.Add("page", physPage);
            return page;
        }

        private IPage<TestRecord> physPage => TestContext.Properties["page"] as IPage<TestRecord>;

        [TestMethod]
        public void InsertInEmpty()
        {
            var page = Create();
            var newRec = new TestRecord { Order = 0 };

            physPage.Expect(k => k.AddRecord(newRec))
                    .Do(new Func<TestRecord, bool>(k =>
                       { k.Reference = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 0 }; return true; }));
            physPage.Replay();
            page.AddRecord(new TestRecord { Order = 0 });

            physPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void UpdateOneRecord()
        {

            var newRec = new TestRecord { Order = 0, Reference = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 0 } };

            var page = Create(newRec);
            physPage.Expect(k => k.GetRecord(null)).IgnoreArguments().Return(newRec);
            physPage.Replay();
            newRec = page.TestGetRecord(newRec.Reference);
            newRec.Order = 1;
            physPage.BackToRecord();
            physPage.Expect(k => k.StoreRecord(newRec));
            physPage.Replay();
            page.StoreRecord(newRec);

            physPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void UpdateWithOrderChange()
        {

            var exRec1 = new TestRecord { Order = 1, Reference = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 0 } };
            var exRec2 = new TestRecord { Order = 2, Reference = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 1 } };
            var exRec3 = new TestRecord { Order = 3, Reference = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 2 } };

            var page = Create(exRec1,exRec2,exRec3);
            physPage.Expect(k => k.GetRecord(null)).IgnoreArguments()
                .Do(new Func<PageRecordReference,TestRecord>(r=>new TestRecord { Order = r.LogicalRecordNum,Reference = r }))
                .Repeat.Any();
            physPage.Replay();
            var newRec = page.TestGetRecord(exRec3.Reference);
            newRec.Order = 0;
            physPage.BackToRecord(BackToRecordOptions.None);
            physPage.Expect(k => k.StoreRecord(newRec));
            physPage.Expect(k => k.SwapRecords(exRec2.Reference, exRec1.Reference));
            physPage.Expect(k => k.SwapRecords(exRec3.Reference, exRec2.Reference));
            physPage.Replay();
            page.StoreRecord(newRec);

            Assert.AreEqual(0, page.First().Order);
            Assert.AreEqual(2, page.Last().Order);

            physPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void DeleteOneRecord()
        {
            var newRec = new TestRecord { Order = 0, Reference = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 0 } };

            var page = Create(newRec);
            physPage.Expect(k => k.GetRecord(null)).IgnoreArguments().Return(newRec);
            physPage.Replay();
            newRec = page.TestGetRecord(newRec.Reference);
            newRec.Order = 1;
            physPage.BackToRecord();
            physPage.Expect(k => k.FreeRecord(newRec));            
            physPage.Replay();
            page.FreeRecord(newRec);

            physPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void DeleteFromMultipleRecords()
        {
            var r1 = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 0 };
            var r2 = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 1 };
            var exRec1 = new TestRecord { Order = 0, Reference = r1 };
            var exRec2 = new TestRecord { Order = 1, Reference = r2 };

            var page = Create(exRec1,exRec2);
            physPage.Expect(k => k.GetRecord(r1)).IgnoreArguments().Return(exRec1);
            physPage.Replay();
            var newRec = page.TestGetRecord(exRec1.Reference);
            
            physPage.BackToRecord(BackToRecordOptions.All);
            physPage.Expect(k => k.Reference).Return(r1.Page).Repeat.Any();
            physPage.Expect(k => k.FreeRecord(newRec));
            physPage.Expect(k => k.SwapRecords(r2, r1));
            physPage.Replay();
            page.FreeRecord(newRec);                       

            physPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void InsertInNotEmpty()
        {
            var r1 = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 0 };
            var exRec = new TestRecord { Order = 1, Reference = r1 };

            var page = Create(exRec);
            var newRec = new TestRecord { Order = 0 };
            var r2 = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = 0 };
            physPage.Expect(k => k.AddRecord(newRec))
                    .Do(new Func<TestRecord, bool>(k =>
                    { k.Reference = r2; return true; }));
            r2.LogicalRecordNum = 1;
            physPage.Expect(k => k.SwapRecords(r2, r1));
            physPage.Replay();
            page.AddRecord(newRec);

            physPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void InsertMultipleInEmpty()
        {
            var page = Create();
            var records = new List<TestRecord>();
            var i = 0;
            physPage.Expect(k => k.SwapRecords(null, null)).IgnoreArguments().Repeat.Any();
            var rnd = new Random();
            foreach(var t in Enumerable.Range(0,10).OrderBy(k=>rnd.Next(10)))
            {
                var newRec = new TestRecord { Order = t };
                var r = new PageRecordReference { Page = new PageReference(0), LogicalRecordNum = i++ };
                physPage.Expect(k => k.AddRecord(newRec))
                        .Do(new Func<TestRecord, bool>(k =>
                        { k.Reference = r; return true; }));
                records.Add(newRec);
            }
            physPage.Replay();

            foreach(var p in records)
            {
                page.AddRecord(p);
            }
            physPage.VerifyAllExpectations();
            physPage.BackToRecord(BackToRecordOptions.None);
            
            physPage.Expect(k => k.GetRecord(null)).IgnoreArguments()
                .Do(new Func<PageRecordReference, TestRecord>(r =>new TestRecord { Order = r.LogicalRecordNum,Reference = r}))
                .Repeat.Any();
            physPage.Replay();
          
            Assert.AreEqual(0, page.First().Order);
            Assert.AreEqual(9, page.Last().Order);
        }
    }
}
