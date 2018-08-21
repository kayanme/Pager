using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.LogicalLevel.Classes;
using System.IO.Paging.LogicalLevel.Classes.OrderedPage;
using System.IO.Paging.LogicalLevel.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO.Paging.PhysicalLevel.Classes.References;
using FakeItEasy;

namespace Test.Paging.LogicalLevel
{
    [TestClass]
    public class OrderedPage
    {
        public TestContext TestContext { get; set; }

        private IOrderedPage<TestRecord,int> Create(params TypedRecord<TestRecord>[] initialState)
        {
            
            var physPage = A.Fake<IPage<TestRecord>>(c=>c.Implements<ILogicalRecordOrderManipulation>());

            var t = A.Fake<IPageManager>();
            var pageNum = new PageReference(0);
            A.CallTo(()=>t.GetRecordAccessor<TestRecord>(pageNum)).Returns(physPage);
            A.CallTo(()=>t.GetSorter<TestRecord>(pageNum)).Returns(physPage as ILogicalRecordOrderManipulation);
            A.CallTo(()=>physPage.IterateRecords()).Returns(initialState.ToArray());
            A.CallTo(()=>physPage.GetRecord(A<PageRecordReference>.Ignored))                
                .Invokes((PageRecordReference r) => initialState.FirstOrDefault(k2 => k2.Reference == r));
            
            var page = new OrderedPage<TestRecord, int>(pageNum,t, k => k.Order,new SortStateContoller{IsSorted = true});
            
            TestContext.Properties.Add("page", physPage);
            TestContext.Properties.Add("manager", t);
            

            return page;
        }

        private IPage<TestRecord> PhysPage => TestContext.Properties["page"] as IPage<TestRecord>;
        private ILogicalRecordOrderManipulation ManPage => TestContext.Properties["page"] as ILogicalRecordOrderManipulation;

        private IPageManager Manager => TestContext.Properties["manager"] as IPageManager;
        

        [TestMethod]
        public void InsertInEmpty()
        {
            var page = Create();
            var newRec = new TestRecord { Order = 0 };

            A.CallTo(()=>PhysPage.AddRecord(newRec))
                    .ReturnsLazily((TestRecord k) =>
                {
                    var t = new TypedRecord<TestRecord>
                    {
                        Reference = new RowKeyPersistentPageRecordReference(0, 0),
                        Data = k
                    };
                  return t;
                });
          
            
            page.AddRecord(new TestRecord { Order = 0 });

            A.CallTo(() => PhysPage.AddRecord(newRec)).MustHaveHappened();
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
            A.CallTo(()=>PhysPage.GetRecord(A<PageRecordReference>.Ignored)).Returns(newRec);
            
            newRec = page.GetRecord(newRec.Reference);
            newRec.Data.Order = 1;                                    
            page.StoreRecord(newRec);

            A.CallTo(() => PhysPage.StoreRecord(newRec)).MustHaveHappened();
            A.CallTo(() => ManPage.DropOrder(newRec.Reference)).MustHaveHappened();

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
            A.CallTo(()=>PhysPage.GetRecord(A<PageRecordReference>.Ignored))
                .ReturnsLazily((PageRecordReference r)=>
                new TypedRecord<TestRecord> { Data = new  TestRecord(r.PersistentRecordNum),Reference = r });
            
            var newRec = page.GetRecord(exRec3.Reference);
            newRec.Data.Order = 0;
            
            
            page.StoreRecord(newRec);

            //Assert.AreEqual(0, page.First().Order);
            //Assert.AreEqual(2, page.Last().Order);

            A.CallTo(()=>PhysPage.StoreRecord(newRec)).MustHaveHappened();
            A.CallTo(()=>ManPage.DropOrder(newRec.Reference)).MustHaveHappened();

        }

        [TestMethod]
        public void DeleteOneRecord()
        {
            var newRec = new TypedRecord<TestRecord> { Data = new TestRecord(0), Reference = new RowKeyPersistentPageRecordReference(0, 0) };

            var page = Create(newRec);
            A.CallTo(() => PhysPage.GetRecord(A<PageRecordReference>.Ignored)).Returns(newRec);

            newRec = page.GetRecord(newRec.Reference);
            newRec.Data.Order = 1;                                         
            page.FreeRecord(newRec);

            A.CallTo(() => PhysPage.FreeRecord(newRec)).MustHaveHappened();
        }

        [TestMethod]
        public void DeleteFromMultipleRecords()
        {
            var r1 = new RowKeyPersistentPageRecordReference(0, 0);
            var r2 = new RowKeyPersistentPageRecordReference(0, 1);
            var exRec1 = new TypedRecord<TestRecord> { Data = new TestRecord(0), Reference = r1 };
            var exRec2 = new TypedRecord<TestRecord> { Data = new TestRecord(1), Reference = r2 };
            
            var page = Create(exRec1,exRec2);
            A.CallTo(()=>PhysPage.GetRecord(A<PageRecordReference>.Ignored)).Returns(exRec1);
                                                
            var newRec = page.GetRecord(exRec1.Reference);
            
            page.FreeRecord(newRec);

            A.CallTo(() => PhysPage.FreeRecord(newRec)).MustHaveHappened();
        }

        [TestMethod]
        public void InsertInNotEmpty()
        {
            var r1 = new RowKeyPersistentPageRecordReference(0, 0);
            var exRec = new TypedRecord<TestRecord> { Data = new TestRecord(1), Reference = r1 };

            var page = Create(exRec);
            var newRec = new TestRecord { Order = 0 };
            var r2 = new RowKeyPersistentPageRecordReference(0,1);
            A.CallTo(()=>PhysPage.AddRecord(newRec))
                    .ReturnsLazily((TestRecord k) =>
                    {var t = new TypedRecord<TestRecord>{ Reference = r2,Data = k}; return t; });


           
            
            page.AddRecord(newRec);

            
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
                A.CallTo(()=>PhysPage.AddRecord(newRec))
                        .ReturnsLazily((TestRecord k) =>
                        { var t2 = new TypedRecord<TestRecord> { Reference = r, Data = k }; return t2; });
                records.Add(newRec);
            }
                        
            foreach(var p in records)
            {
                page.AddRecord(p);
            }
            //A.CallTo(() => PhysPage.Flush()).MustHaveHappened();                        
            A.CallTo(()=>PhysPage.GetRecord(A<PageRecordReference>.Ignored))
                .ReturnsLazily((PageRecordReference r) =>
                new TypedRecord<TestRecord> { Data = new TestRecord(r.PersistentRecordNum),Reference = r});
            
          
            //Assert.AreEqual(0, page.First().Order);
            //Assert.AreEqual(9, page.Last().Order);
        }

        private TypedRecord<TestRecord> CreateRecord(ushort num)
        {
            var r1 = new RowKeyPersistentPageRecordReference(0, num);
            var exRec = new TypedRecord<TestRecord> { Data = new TestRecord(num), Reference = r1 };
            return exRec;
        }

        [TestMethod]
        public void SearchForKeyWhenPresent()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(3);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                  .Returns(searcher);
            A.CallTo(() => searcher.Current).ReturnsNextFromSequence(r2,r3);
            A.CallTo(() => searcher.MoveRight()).Returns(true);            



            var rec = page.FindByKey(3);
            Assert.AreEqual(r3.Data.Order, rec.Data.Order);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                .Then(                                
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                 .Then(
                    A.CallTo(() => searcher.MoveRight()).MustHaveHappened())
                 .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                 .Then(
                    A.CallTo(() => searcher.Dispose()).MustHaveHappened());

        }

        [TestMethod]
        public void SearchForKeyWhenNotPresent_TooBig()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(3);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .Returns(searcher);
            A.CallTo(() => searcher.Current).ReturnsNextFromSequence(r2,r3);
            A.CallTo(() => searcher.MoveRight()).ReturnsNextFromSequence(true,false);            




            var rec = page.FindByKey(4);
            Assert.IsNull(rec);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveRight()).MustHaveHappened())
                .Then(                    
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveRight()).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Dispose()).MustHaveHappened());


        }

        [TestMethod]
        public void SearchForKeyWhenNotPresent_TooSmall()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(3);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();
         
                A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                    .Returns(searcher);
                A.CallTo(() => searcher.Current).ReturnsNextFromSequence(r2,r1);
                A.CallTo(() => searcher.MoveLeft()).ReturnsNextFromSequence(true,false);
                
            

            
                var rec = page.FindByKey(0);
                Assert.IsNull(rec);
            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveLeft()).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveLeft()).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Dispose()).MustHaveHappened());
        }

        [TestMethod]
        public void SearchForTheMostLesserKey()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(4);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .Returns(searcher);
            A.CallTo(() => searcher.Current).ReturnsNextFromSequence(r2,r3);
            A.CallTo(() => searcher.MoveRight()).Returns(true);            
            A.CallTo(() => searcher.MoveLeft()).Returns(false);
            

            var rec = page.FindTheMostLesser(3, true);
            Assert.AreEqual(2, rec.Data.Order);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                   .Then(
                        A.CallTo(() => searcher.Current).MustHaveHappened())
                    .Then(
                        A.CallTo(() => searcher.MoveRight()).MustHaveHappened())
                    .Then(
                        A.CallTo(() => searcher.Current).MustHaveHappened())
                    .Then(
                        A.CallTo(() => searcher.MoveLeft()).MustHaveHappened())
                    .Then(
                        A.CallTo(() => searcher.Dispose()).MustHaveHappened());
        }

        [TestMethod]
        public void SearchForTheMostLesserNotEqualKey()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(3);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .Returns(searcher);
            A.CallTo(() => searcher.Current).Returns(r2);
            A.CallTo(() => searcher.LeftOfCurrent).Returns(r1);
            



            var rec = page.FindTheMostLesser(2, false);
            Assert.AreEqual(1, rec.Data.Order);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .MustHaveHappened()
                  .Then(
                        A.CallTo(() => searcher.Current).MustHaveHappened())
                  .Then(
                        A.CallTo(() => searcher.LeftOfCurrent).MustHaveHappened())
                   .Then(
                        A.CallTo(() => searcher.Dispose()).MustHaveHappened());
        }

        [TestMethod]
        public void SearchForTheMostLesserEqualKey()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(3);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .Returns(searcher);
            A.CallTo(() => searcher.Current).ReturnsNextFromSequence(r2,r3);
            A.CallTo(() => searcher.MoveRight()).Returns(true);                       

            var rec = page.FindTheMostLesser(3, true);
            Assert.AreEqual(3, rec.Data.Order);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveRight()).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Dispose()).MustHaveHappened());
        }




        [TestMethod]
        public void SearchForTheLessGreaterKey()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(4);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .Returns(searcher);
            A.CallTo(() => searcher.Current).ReturnsNextFromSequence(r2,r3);
            A.CallTo(() => searcher.MoveRight()).ReturnsNextFromSequence(true,false);                                   

            var rec = page.FindTheLessGreater(3, true);
            Assert.AreEqual(4, rec.Data.Order);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveRight()).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveLeft()).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Dispose()).MustHaveHappened());
        }

        [TestMethod]
        public void SearchForTheLessGreaterNotEqualKey()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(3);
            var r3 = CreateRecord(4);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();


            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .Returns(searcher);
            A.CallTo(() => searcher.Current).Returns(r2);
            A.CallTo(() => searcher.RightOfCurrent).Returns(r3);
            

            var rec = page.FindTheLessGreater(3, false);
            Assert.AreEqual(4, rec.Data.Order);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.RightOfCurrent).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Dispose()).MustHaveHappened());
        }

        [TestMethod]
        public void SearchForTheLessGreaterEqualKey()
        {

            var r1 = CreateRecord(1);
            var r2 = CreateRecord(2);
            var r3 = CreateRecord(3);
            var page = Create(r1, r2, r3);

            var searcher = A.Fake<IBinarySearcher<TestRecord>>();

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0)))
                                .Returns(searcher);
            A.CallTo(() => searcher.Current).ReturnsNextFromSequence(r2,r3);
            A.CallTo(() => searcher.MoveRight()).Returns(true);            
            

            var rec = page.FindTheLessGreater(3, true);
            Assert.AreEqual(3, rec.Data.Order);

            A.CallTo(() => Manager.GetBinarySearchForPage<TestRecord>(new PageReference(0))).MustHaveHappened()
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.MoveRight()).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Current).MustHaveHappened())
                .Then(
                    A.CallTo(() => searcher.Dispose()).MustHaveHappened());
        }
    }
}
