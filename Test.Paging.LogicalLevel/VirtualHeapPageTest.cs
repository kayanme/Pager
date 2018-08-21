using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.LogicalLevel.Classes.ContiniousHeapPage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using FakeItEasy;

namespace Test.Paging.LogicalLevel
{
    [TestClass]
    public class VirtualHeapPageTest
    {
        public TestContext TestContext { get; set; }
        private byte _headerType = 2;
        private byte _pageType = 1;
        private IPage<TestRecord> CreatePage(params HeapHeader[] initialPages)
        {
            var tp = new VirtualContiniousPage<TestRecord>(physManager, _pageType, _headerType);
            return tp;
        }

        [TestInitialize]
        public void TestInitialize()
        {

            physManager = A.Fake<IPageManager>();
            heapHeaders = A.Fake<IPage<HeapHeader>>(c => c.Implements<IPhysicalRecordManipulation>());
            //A.CallTo(() => (heapHeaders as IPhysicalRecordManipulation).Flush());
        }



        private IPageManager physManager
        {
            get => TestContext.Properties["pm"] as IPageManager;
            set => TestContext.Properties["pm"] = value;
        }

        private IPage<HeapHeader> heapHeaders
        {
            get => TestContext.Properties["hh"] as IPage<HeapHeader>;
            set => TestContext.Properties["hh"] = value;
        }

        [TestMethod]
        public void AddInFull()
        {


            A.CallTo(() => physManager.IteratePages(_headerType)).Returns(new[] { new PageReference(1) });
            A.CallTo(() => physManager.GetRecordAccessor<HeapHeader>(new PageReference(1))).Returns(heapHeaders);
            A.CallTo(() => heapHeaders.IterateRecords())
                                .Returns(new[] { new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } } });
            A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0)))
                                .Returns(new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } });

            var page = CreatePage();

            A.CallTo(() => physManager.IteratePages(_headerType)).MustHaveHappened()
                .Then(
                    A.CallTo(() => physManager.GetRecordAccessor<HeapHeader>(new PageReference(1))).MustHaveHappened())
                .Then(
                    A.CallTo(() => heapHeaders.IterateRecords()).MustHaveHappened())
                .Then(
                    A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0))).MustHaveHappened());
            var tr = new TestRecord();
            var realPage = A.Fake<IPage<TestRecord>>();
            var realPage2 = A.Fake<IPage<TestRecord>>();



            var pageInfo = A.Fake<IPageInfo>();
            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).Returns(realPage);            
            A.CallTo(() => realPage.AddRecord(tr)).Returns(null);          
            A.CallTo(() => heapHeaders.IterateRecords())
                                .Returns(new[] { new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = 1, LogicalPageNum = 5 } } });
            A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0)))
                                .Returns(new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = 1, LogicalPageNum = 5 } });
            A.CallTo(() => physManager.CreatePage(_pageType)).Returns(new PageReference(6));

            A.CallTo(() => heapHeaders.AddRecord(new HeapHeader { Fullness = 0, LogicalPageNum = 6 }))
                                .Returns(new TypedRecord<HeapHeader>());

            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(6))).Returns(realPage2);
            A.CallTo(() => realPage2.AddRecord(tr)).Returns(new TypedRecord<TestRecord>());
            A.CallTo(() => physManager.GetPageInfo(new PageReference(6))).Returns(pageInfo);
            A.CallTo(() => pageInfo.PageFullness).Returns(.1);
           
         
            Assert.IsNotNull(page.AddRecord(tr));

            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).MustHaveHappened()
                .Then(
                    A.CallTo(() => realPage.AddRecord(tr)).MustHaveHappened())
                .Then(
                    A.CallTo(() => heapHeaders.StoreRecord(new TypedRecord<HeapHeader> { Data = new HeapHeader { LogicalPageNum = 5, Fullness = 1 } })).MustHaveHappened())
                .Then(
                    A.CallTo(() => heapHeaders.IterateRecords()).MustHaveHappened())
                .Then(
                    A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0))).MustHaveHappened())
                .Then(
                    A.CallTo(() => physManager.CreatePage(_pageType)).MustHaveHappened())
                .Then(
                    A.CallTo(() => heapHeaders.AddRecord(new HeapHeader { Fullness = 0, LogicalPageNum = 6 })).MustHaveHappened())
                .Then(
                    A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(6))).MustHaveHappened())
                .Then(
                    A.CallTo(() => realPage2.AddRecord(tr)).MustHaveHappened())
                .Then(
                    A.CallTo(() => physManager.GetPageInfo(new PageReference(6))).MustHaveHappened())
                .Then(
                    A.CallTo(() => pageInfo.PageFullness).MustHaveHappened())
                 .Then(
                    A.CallTo(() => heapHeaders.StoreRecord(new TypedRecord<HeapHeader> { Data = new HeapHeader { LogicalPageNum = 6, Fullness = .1 } })).MustHaveHappened());

        }

        [TestMethod]
        public void AddInEmpty()
        {
            var realPage = A.Fake<IPage<TestRecord>>();
            var pageInfo = A.Fake<IPageInfo>();

            A.CallTo(() => physManager.IteratePages(_headerType)).Returns(new[] { new PageReference(1) });
            A.CallTo(() => physManager.GetRecordAccessor<HeapHeader>(new PageReference(1))).Returns(heapHeaders);
            A.CallTo(() => heapHeaders.IterateRecords())
                                .Returns(new TypedRecord<HeapHeader>[0]);
            A.CallTo(() => physManager.CreatePage(_pageType)).Returns(new PageReference(5));
            A.CallTo(() => heapHeaders.AddRecord(A<HeapHeader>.That.Matches(k2 => k2.Fullness == 0 && k2.LogicalPageNum == 5)))
                                .Returns(new TypedRecord<HeapHeader>());


            var page = CreatePage();
            var tr = new TestRecord();


            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).Returns(realPage);
            A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).Returns(pageInfo);
            A.CallTo(() => realPage.AddRecord(tr)).Returns(new TypedRecord<TestRecord>());
            A.CallTo(() => pageInfo.PageFullness).Returns(.1);
           
            

            Assert.IsNotNull(page.AddRecord(tr));

            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).MustHaveHappened()
                .Then(
                   A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).MustHaveHappened())
                .Then(
                   A.CallTo(() => realPage.AddRecord(tr)).MustHaveHappened())
                .Then(
                   A.CallTo(() => pageInfo.PageFullness).MustHaveHappened())
                .Then(
                    A.CallTo(() => heapHeaders.StoreRecord(new TypedRecord<HeapHeader> { Data = new HeapHeader { LogicalPageNum = 5, Fullness = .1 } })).MustHaveHappened())
                .Then(
                    A.CallTo(() => realPage.Dispose()).MustHaveHappened()
                );

        }

        [TestMethod]
        public void Modify()
        {
            var realPage = A.Fake<IPage<TestRecord>>();
            var pageInfo = A.Fake<IPageInfo>();

            A.CallTo(() => physManager.IteratePages(_headerType)).Returns(new[] { new PageReference(1) });
            A.CallTo(() => physManager.GetRecordAccessor<HeapHeader>(new PageReference(1))).Returns(heapHeaders);
            A.CallTo(() => heapHeaders.IterateRecords())
                                .Returns(new[] { new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } } });
            A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0)))
                                .Returns(new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } });


            var page = CreatePage();
            var reference = new LogicalPositionPersistentPageRecordReference(5, 4);
            var tr = new TypedRecord<TestRecord> { Reference = reference };
           
            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).Returns(realPage);
            A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).Returns(pageInfo);
            A.CallTo(() => pageInfo.RegisteredPageType).Returns(_pageType);
            

            page.StoreRecord(tr);

            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).MustHaveHappened();
            A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).MustHaveHappened();
            A.CallTo(() => pageInfo.RegisteredPageType).MustHaveHappened();
            A.CallTo(() => realPage.StoreRecord(tr)).MustHaveHappened();

        }

        [TestMethod]
        public void Free()
        {
            var realPage = A.Fake<IPage<TestRecord>>();
            var pageInfo = A.Fake<IPageInfo>();

            A.CallTo(() => physManager.IteratePages(_headerType)).Returns(new[] { new PageReference(1) });
            A.CallTo(() => physManager.GetRecordAccessor<HeapHeader>(new PageReference(1))).Returns(heapHeaders);
            A.CallTo(() => heapHeaders.IterateRecords())
                                .Returns(new[] { new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } } });
            A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0)))
                                .Returns(new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } });

            var page = CreatePage();
            var reference = new LogicalPositionPersistentPageRecordReference(5, 4);
            var tr = new TypedRecord<TestRecord> { Reference = reference };

            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).Returns(realPage);
            A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).Returns(pageInfo);
            A.CallTo(() => pageInfo.RegisteredPageType).Returns(_pageType);            
            A.CallTo(() => pageInfo.Reference).Returns(new PageReference(5));
            A.CallTo(() => heapHeaders.IterateRecords())
                                .Returns(new[] { new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } } });
            A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0)))
                                .Returns(new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } });

            A.CallTo(() => pageInfo.PageFullness).Returns(0.2);
            


            page.FreeRecord(tr);

            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).MustHaveHappened();
            A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).MustHaveHappened();
            A.CallTo(() => pageInfo.RegisteredPageType).MustHaveHappened();
            A.CallTo(() => realPage.FreeRecord(tr)).MustHaveHappened();
            A.CallTo(() => pageInfo.Reference).MustHaveHappenedOnceOrMore();
            A.CallTo(() => heapHeaders.IterateRecords()).MustHaveHappened();
            A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0))).MustHaveHappened();

            A.CallTo(() => pageInfo.PageFullness).MustHaveHappened();
            A.CallTo(() => heapHeaders.StoreRecord(new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .2, LogicalPageNum = 5 } })).MustHaveHappened();
        }

        [TestMethod]
        public void Retrieve()
        {
            var realPage = A.Fake<IPage<TestRecord>>();
            var pageInfo = A.Fake<IPageInfo>();

            A.CallTo(() => physManager.IteratePages(_headerType)).Returns(new[] { new PageReference(1) });
            A.CallTo(() => physManager.GetRecordAccessor<HeapHeader>(new PageReference(1))).Returns(heapHeaders);
            A.CallTo(() => heapHeaders.IterateRecords())
                                .Returns(new[] { new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } } });
            A.CallTo(() => heapHeaders.GetRecord(new LogicalPositionPersistentPageRecordReference(1, 0)))
                                .Returns(new TypedRecord<HeapHeader> { Data = new HeapHeader { Fullness = .9, LogicalPageNum = 5 } });

            var page = CreatePage();
            var reference = new LogicalPositionPersistentPageRecordReference(5, 4);
            var tr = new TypedRecord<TestRecord> { Reference = reference };
         
            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).Returns(realPage);
            A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).Returns(pageInfo);
            A.CallTo(() => pageInfo.RegisteredPageType).Returns(_pageType);
            A.CallTo(() => realPage.GetRecord(reference)).Returns(tr);

            Assert.AreEqual(tr, page.GetRecord(reference));

            A.CallTo(() => physManager.GetRecordAccessor<TestRecord>(new PageReference(5))).MustHaveHappened();
            A.CallTo(() => physManager.GetPageInfo(new PageReference(5))).MustHaveHappened();
            A.CallTo(() => pageInfo.RegisteredPageType).MustHaveHappened();
            A.CallTo(() => realPage.GetRecord(reference)).MustHaveHappened();

        }

        private void TestHeader(double fullness, uint pageNum)
        {
            var hh = new HeapHeader { Fullness = fullness, LogicalPageNum = pageNum };
            var data = new byte[hh.Size];
            var t = new HeapHeader();
            t.FillBytes(ref hh, data);

            t.FillFromBytes(data, ref t);
            Assert.AreEqual(fullness, t.Fullness);
            Assert.AreEqual(pageNum, t.LogicalPageNum);
        }

        [TestMethod]
        public void HeapHeaderTest()
        {
            TestHeader(0, 1);
            TestHeader(.5, 10);
            TestHeader(.8, 100000);
            TestHeader(.95, 45444);

        }
    }
}

