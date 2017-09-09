using System;
using System.Collections.Generic;
using System.Linq;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes;
using FIle.Paging.LogicalLevel.Classes.OrderedPage;
using FIle.Paging.LogicalLevel.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Paging.LogicalLevel
{
    [TestClass]
    public class OrderedPage
    {
        public TestContext TestContext { get; set; }

        private IOrderedPage<TestRecord,int> Create(params TypedRecord<TestRecord>[] initialState)
        {
            var rep = new MockRepository();
            var physPage = rep.StrictMultiMock<IPage<TestRecord>>(typeof(ILogicalRecordOrderManipulation));

            var t = rep.StrictMock<IPageManager>();
            var pageNum = new PageReference(0);
            t.Expect(k => k.GetRecordAccessor<TestRecord>(pageNum)).Return(physPage);
            t.Expect(k => k.GetSorter<TestRecord>(pageNum)).Return(physPage as ILogicalRecordOrderManipulation);
            physPage.Expect(k => k.IterateRecords()).Return(initialState.ToArray());
            physPage.Expect(k => k.GetRecord(null))
                .IgnoreArguments()
                .Do(new Func<PageRecordReference, TypedRecord<TestRecord>>
                    (r => initialState.FirstOrDefault(k2 => k2.Reference == r))).Repeat.Any();
            physPage.Replay();
            var page = new OrderedPage<TestRecord, int>(pageNum,t, k => k.Order,new SortStateContoller());
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
                    .Do(new Func<TestRecord, TypedRecord<TestRecord>>(k =>
                {
                    var t = new TypedRecord<TestRecord>
                    {
                        Reference = new RowKeyPersistentPageRecordReference(0, 0),
                        Data = k
                    };
                  return t;
                }));
          
            PhysPage.Replay();
            page.AddRecord(new TestRecord { Order = 0 });

            PhysPage.VerifyAllExpectations();
        }

        [TestMethod]
        public void UpdateOneRecord()
        {

            var newRec = new TypedRecord<TestRecord>
            {
                
                Data = new TestRecord{Order = 0},
                Reference = new RowKeyPersistentPageRecordReference(0, 0 )
            };

            var page = Create(newRec);
            PhysPage.Expect(k => k.GetRecord(null)).IgnoreArguments().Return(newRec);
            PhysPage.Replay();
            newRec = page.GetRecord(newRec.Reference);
            newRec.Data.Order = 1;
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

            var exRec1 = new TypedRecord<TestRecord>
            {
               Data = new TestRecord(1), Reference = new RowKeyPersistentPageRecordReference(0,0)
            };
            var exRec2 = new TypedRecord<TestRecord> { Data = new TestRecord(2), Reference = new RowKeyPersistentPageRecordReference(0, 1) };
            var exRec3 = new TypedRecord<TestRecord> { Data = new TestRecord(3), Reference = new RowKeyPersistentPageRecordReference(0, 2) };

            var page = Create(exRec1,exRec2,exRec3);
            PhysPage.Expect(k => k.GetRecord(null)).IgnoreArguments()
                .Do(new Func<PageRecordReference, TypedRecord<TestRecord>>(r=>
                new TypedRecord<TestRecord> { Data = new  TestRecord(r.PersistentRecordNum),Reference = r }))
                .Repeat.Any();
            PhysPage.Replay();
            var newRec = page.GetRecord(exRec3.Reference);
            newRec.Data.Order = 0;
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
            var newRec = new TypedRecord<TestRecord> { Data = new TestRecord(0), Reference = new RowKeyPersistentPageRecordReference(0, 0) };

            var page = Create(newRec);
            PhysPage.Expect(k => k.GetRecord(null)).IgnoreArguments().Return(newRec);
            PhysPage.Replay();
            newRec = page.GetRecord(newRec.Reference);
            newRec.Data.Order = 1;
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
            var exRec1 = new TypedRecord<TestRecord> { Data = new TestRecord(0), Reference = r1 };
            var exRec2 = new TypedRecord<TestRecord> { Data = new TestRecord(1), Reference = r2 };
            
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
            var exRec = new TypedRecord<TestRecord> { Data = new TestRecord(1), Reference = r1 };

            var page = Create(exRec);
            var newRec = new TestRecord { Order = 0 };
            var r2 = new RowKeyPersistentPageRecordReference(0,1);
            PhysPage.Expect(k => k.AddRecord(newRec))
                    .Do(new Func<TestRecord, TypedRecord<TestRecord>>(k =>
                    {var t = new TypedRecord<TestRecord>{ Reference = r2,Data = k}; return t; }));


           
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
                        .Do(new Func<TestRecord, TypedRecord<TestRecord>>(k =>
                        { var t2 = new TypedRecord<TestRecord> { Reference = r, Data = k }; return t2; }));
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
                .Do(new Func<PageRecordReference, TypedRecord<TestRecord>>(r =>
                new TypedRecord<TestRecord> { Data = new TestRecord(r.PersistentRecordNum),Reference = r}))
                .Repeat.Any();
            PhysPage.Replay();
          
            //Assert.AreEqual(0, page.First().Order);
            //Assert.AreEqual(9, page.Last().Order);
        }
    }
}
