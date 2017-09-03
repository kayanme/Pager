using System;
using System.Collections.Generic;
using System.Linq;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Classes;
using FIle.Paging.LogicalLevel.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Paging.LogicalLevel
{
    [TestClass]
    public class OrderedPage
    {
        public TestContext TestContext { get; set; }

        private IOrderedPage<TestRecord> Create(params TestRecord[] initialState)
        {
            var rep = new MockRepository();
            var physPage = rep.StrictMultiMock<IPage<TestRecord>>(typeof(ILogicalRecordOrderManipulation));

            
            physPage.Expect(k => k.IterateRecords()).Return(initialState.Select(k=>k.Reference).ToArray());
            physPage.Expect(k => k.GetRecord(null))
                .IgnoreArguments()
                .Do(new Func<PageRecordReference, TestRecord>
                    (r => initialState.FirstOrDefault(k2 => k2.Reference == r))).Repeat.Any();
            physPage.Replay();
            var page = new OrderedPage<TestRecord, int>(physPage, physPage as ILogicalRecordOrderManipulation, k => k.Order);
            physPage.BackToRecord();
         

            TestContext.Properties.Add("page", physPage);
            
            return page;
        }

        private IPage<TestRecord> PhysPage => TestContext.Properties["page"] as IPage<TestRecord>;
        private ILogicalRecordOrderManipulation ManPage => TestContext.Properties["page"] as ILogicalRecordOrderManipulation;

        [TestMethod]
        public void InsertInEmpty()
        {
            var page = Create();
            var newRec = new TestRecord { Order = 0 };

            PhysPage.Expect(k => k.AddRecord(newRec))
                    .Do(new Func<TestRecord, bool>(k =>
                       { k.Reference = new RowKeyPersistentPageRecordReference(0,0 ); return true; }));
          
            PhysPage.Replay();
            page.AddRecord(new TestRecord { Order = 0 });

            PhysPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void UpdateOneRecord()
        {

            var newRec = new TestRecord { Order = 0, Reference = new RowKeyPersistentPageRecordReference(0, 0 ) };

            var page = Create(newRec);
            PhysPage.Expect(k => k.GetRecord(null)).IgnoreArguments().Return(newRec);
            PhysPage.Replay();
            newRec = page.GetRecord(newRec.Reference);
            newRec.Order = 1;
            PhysPage.BackToRecord();
            PhysPage.Expect(k => k.StoreRecord(newRec));
            ManPage.Expect(k => k.DropOrder(newRec.Reference));
          
            PhysPage.Replay();
            page.StoreRecord(newRec);

            PhysPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void UpdateWithOrderChange()
        {

            var exRec1 = new TestRecord { Order = 1, Reference = new RowKeyPersistentPageRecordReference(0,0) };
            var exRec2 = new TestRecord { Order = 2, Reference = new RowKeyPersistentPageRecordReference(0, 1) };
            var exRec3 = new TestRecord { Order = 3, Reference = new RowKeyPersistentPageRecordReference(0, 2) };

            var page = Create(exRec1,exRec2,exRec3);
            PhysPage.Expect(k => k.GetRecord(null)).IgnoreArguments()
                .Do(new Func<PageRecordReference,TestRecord>(r=>new TestRecord { Order = r.PersistentRecordNum,Reference = r }))
                .Repeat.Any();
            PhysPage.Replay();
            var newRec = page.GetRecord(exRec3.Reference);
            newRec.Order = 0;
            PhysPage.BackToRecord(BackToRecordOptions.None);
            PhysPage.Expect(k => k.StoreRecord(newRec));
            ManPage.Expect(k => k.DropOrder(newRec.Reference));
            PhysPage.Replay();
            page.StoreRecord(newRec);

            //Assert.AreEqual(0, page.First().Order);
            //Assert.AreEqual(2, page.Last().Order);

            PhysPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void DeleteOneRecord()
        {
            var newRec = new TestRecord { Order = 0, Reference = new RowKeyPersistentPageRecordReference(0, 0) };

            var page = Create(newRec);
            PhysPage.Expect(k => k.GetRecord(null)).IgnoreArguments().Return(newRec);
            PhysPage.Replay();
            newRec = page.GetRecord(newRec.Reference);
            newRec.Order = 1;
            PhysPage.BackToRecord();
            PhysPage.Expect(k => k.FreeRecord(newRec));
      
            PhysPage.Replay();
            page.FreeRecord(newRec);

            PhysPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void DeleteFromMultipleRecords()
        {
            var r1 = new RowKeyPersistentPageRecordReference(0, 0);
            var r2 = new RowKeyPersistentPageRecordReference(0, 1);
            var exRec1 = new TestRecord { Order = 0, Reference = r1 };
            var exRec2 = new TestRecord { Order = 1, Reference = r2 };
            
            var page = Create(exRec1,exRec2);
            PhysPage.Expect(k => k.GetRecord(r1)).IgnoreArguments().Return(exRec1);
            
            
            PhysPage.Replay();
            
            var newRec = page.GetRecord(exRec1.Reference);
            
            PhysPage.BackToRecord(BackToRecordOptions.All);
        
            PhysPage.Expect(k => k.FreeRecord(newRec));

        
            PhysPage.Replay();
            page.FreeRecord(newRec);                       

            PhysPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void InsertInNotEmpty()
        {
            var r1 = new RowKeyPersistentPageRecordReference(0, 0);
            var exRec = new TestRecord { Order = 1, Reference = r1 };

            var page = Create(exRec);
            var newRec = new TestRecord { Order = 0 };
            var r2 = new RowKeyPersistentPageRecordReference(0,1);
            PhysPage.Expect(k => k.AddRecord(newRec))
                    .Do(new Func<TestRecord, bool>(k =>
                    { k.Reference = r2; return true; }));


           
            PhysPage.Replay();
            page.AddRecord(newRec);

            PhysPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void InsertMultipleInEmpty()
        {
            var page = Create();
            var records = new List<TestRecord>();
            ushort i = 0;
           
            var rnd = new Random();
            foreach(var t in Enumerable.Range(0,10).OrderBy(k=>rnd.Next(10)))
            {
                var newRec = new TestRecord { Order = t };
                var r = new RowKeyPersistentPageRecordReference(0, i++);
                PhysPage.Expect(k => k.AddRecord(newRec))
                        .Do(new Func<TestRecord, bool>(k =>
                        { k.Reference = r; return true; }));
                records.Add(newRec);
            }
            PhysPage.Expect(k => k.Flush()).Repeat.Any();
            PhysPage.Replay();

            foreach(var p in records)
            {
                page.AddRecord(p);
            }
            PhysPage.VerifyAllExpectations();
            PhysPage.BackToRecord(BackToRecordOptions.None);
            
            PhysPage.Expect(k => k.GetRecord(null)).IgnoreArguments()
                .Do(new Func<PageRecordReference, TestRecord>(r =>new TestRecord { Order = r.PersistentRecordNum,Reference = r}))
                .Repeat.Any();
            PhysPage.Replay();
          
            //Assert.AreEqual(0, page.First().Order);
            //Assert.AreEqual(9, page.Last().Order);
        }
    }
}
