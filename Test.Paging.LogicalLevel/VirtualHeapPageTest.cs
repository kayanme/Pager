using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

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
            mocks = new MockRepository();
            physManager = mocks.StrictMock<IPageManager>();
            heapHeaders = mocks.StrictMultiMock<IPage<HeapHeader>>(typeof(IPhysicalLevelManipulation));
            (heapHeaders as IPhysicalLevelManipulation).Expect(k => k.Flush()).Repeat.Any();
        }

        private MockRepository mocks
        {
            get => TestContext.Properties["mr"] as MockRepository;
            set => TestContext.Properties["mr"] = value;
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

            using (mocks.Record())
            using (mocks.Ordered())          
            {
                physManager.Expect(k => k.IteratePages(_headerType)).Return(new[] { heapHeaders });
                heapHeaders.Expect(k => k.IterateRecords())
                    .Return(new[] { new PageRecordReference(1, 0) });
                heapHeaders.Expect(k => k.GetRecord(new PageRecordReference(1, 0)))
                    .Return(new HeapHeader { Fullness = .9, LogicalPageNum = 5 });
            }
            IPage<TestRecord> page;
            using (mocks.Playback())
                 page = CreatePage();
            var tr = new TestRecord();
            using (mocks.Record())
            using (mocks.Ordered())           
            {               
                var realPage = mocks.StrictMock<IPage<TestRecord>>();
                var realPage2 = mocks.StrictMock<IPage<TestRecord>>();
                physManager.Expect(k => k.RetrievePage(new PageReference(5))).Return(realPage);
                realPage.Expect(k => k.AddRecord(tr)).Return(false);
                realPage.Expect(k => k.Dispose()).Repeat.Once();
                heapHeaders.Expect(k => k.StoreRecord(new HeapHeader { LogicalPageNum = 5, Fullness = 1 }));

                heapHeaders.Expect(k => k.IterateRecords())
                    .Return(new[] { new PageRecordReference(1, 0) });
                heapHeaders.Expect(k => k.GetRecord(new PageRecordReference(1, 0)))
                    .Return(new HeapHeader { Fullness = 1, LogicalPageNum = 5 });

                physManager.Expect(k => k.CreatePage(_pageType)).Return(realPage2);
                realPage2.Expect(k => k.Reference).Return(new PageReference(6));
                heapHeaders
                    .Expect(k => k.AddRecord(new HeapHeader{Fullness = 0 , LogicalPageNum = 6}))
                    .Return(true);
              
                physManager.Expect(k => k.RetrievePage(new PageReference(6))).Return(realPage2);
                realPage2.Expect(k => k.AddRecord(tr)).Return(true);
                realPage2.Expect(k => k.PageFullness).Return(.1);
                heapHeaders.Expect(k => k.StoreRecord(new HeapHeader { LogicalPageNum = 6, Fullness = .1 }));
                realPage2.Expect(k => k.Dispose()).Repeat.Once();
            }
            using (mocks.Playback())
                Assert.IsTrue(page.AddRecord(tr));
           
        }

        [TestMethod]
        public void AddInEmpty()
        {
            var realPage = mocks.StrictMock<IPage<TestRecord>>();
            using (mocks.Record())           
            {
                physManager.Expect(k => k.IteratePages(_headerType)).Return(new []{heapHeaders});
                heapHeaders.Expect(k => k.IterateRecords())
                    .Return(new PageRecordReference[0]);
                physManager.Expect(k => k.CreatePage(_pageType)).Return(realPage);
                realPage.Expect(k => k.Reference).Return(new PageReference(5));
                heapHeaders
                    .Expect(k => k.AddRecord(Arg<HeapHeader>.Matches(k2 => k2.Fullness == 0 && k2.LogicalPageNum == 5)))
                    .Return(true);
            

            }
            IPage<TestRecord> page;
         
            using (mocks.Playback())           
                page = CreatePage();
            var tr = new TestRecord();
        
            using (mocks.Record())        
            {
             
                physManager.Expect(k => k.RetrievePage(new PageReference(5))).Return(realPage);            
                realPage.Expect(k => k.AddRecord(tr)).Return(true);
                realPage.Expect(k => k.PageFullness).Return(.1);
                heapHeaders.Expect(k => k.StoreRecord(new HeapHeader { LogicalPageNum = 5, Fullness = .1 }));
                realPage.Expect(k => k.Dispose()).Repeat.Once();
            }

           
            using (mocks.Playback())
            {
                Assert.IsTrue(page.AddRecord(tr));
            }
            mocks.VerifyAll();
        }

        [TestMethod]
        public void Modify()
        {
            var realPage = mocks.StrictMock<IPage<TestRecord>>();
            using (mocks.Record())
            {
                physManager.Expect(k => k.IteratePages(_headerType)).Return(new[] { heapHeaders });
                heapHeaders.Expect(k => k.IterateRecords())
                    .Return(new[] { new PageRecordReference(1, 0) });
                heapHeaders.Expect(k => k.GetRecord(new PageRecordReference(1, 0)))
                    .Return(new HeapHeader { Fullness = .9, LogicalPageNum = 5 });
                       
            }
            IPage<TestRecord> page;

            using (mocks.Playback())
                page = CreatePage();
            var reference =  new PageRecordReference(5,4);
            var tr = new TestRecord{ Reference = reference};

            using (mocks.Record())
            {
                physManager.Expect(k => k.RetrievePage(new PageReference(5))).Return(realPage);
                realPage.Expect(k => k.RegisteredPageType).Return(_pageType);
                realPage.Expect(k => k.StoreRecord(tr));
                realPage.Expect(k => k.Dispose());
            }

            using (mocks.Playback())
            {
               page.StoreRecord(tr);
            }
           
        }

        [TestMethod]
        public void Free()
        {
            var realPage = mocks.StrictMock<IPage<TestRecord>>();
            using (mocks.Record())
            {
                physManager.Expect(k => k.IteratePages(_headerType)).Return(new[] { heapHeaders });
                heapHeaders.Expect(k => k.IterateRecords())
                    .Return(new[] { new PageRecordReference(1, 0) });
                heapHeaders.Expect(k => k.GetRecord(new PageRecordReference(1, 0)))
                    .Return(new HeapHeader { Fullness = .9, LogicalPageNum = 5 });
             
            }
            IPage<TestRecord> page;

            using (mocks.Playback())
                page = CreatePage();
            var reference = new PageRecordReference(5,4);
            var tr = new TestRecord { Reference = reference };

            using (mocks.Record())
            {
                physManager.Expect(k => k.RetrievePage(new PageReference(5))).Return(realPage);
                realPage.Expect(k => k.RegisteredPageType).Return(_pageType);
                realPage.Expect(k => k.FreeRecord(tr));
                realPage.Expect(k => k.Reference).Return(new PageReference(5)).Repeat.Any();
                heapHeaders.Expect(k => k.IterateRecords())
                    .Return(new[] { new PageRecordReference(1, 0) });
                heapHeaders.Expect(k => k.GetRecord(new PageRecordReference(1, 0)))
                    .Return(new HeapHeader { Fullness = .9, LogicalPageNum = 5 });
              
                realPage.Expect(k => k.PageFullness).Return(0.2);
                heapHeaders.Expect(k => k.StoreRecord(new HeapHeader {Fullness = .2, LogicalPageNum = 5}));
                realPage.Expect(k => k.Dispose());
            }

            using (mocks.Playback())
            {
                page.FreeRecord(tr);
            }

        }

        [TestMethod]
        public void Retrieve()
        {
            var realPage = mocks.StrictMock<IPage<TestRecord>>();
            using (mocks.Record())
            {
                physManager.Expect(k => k.IteratePages(_headerType)).Return(new[] { heapHeaders });
                heapHeaders.Expect(k => k.IterateRecords())
                    .Return(new[] { new PageRecordReference(1, 0) });
                heapHeaders.Expect(k => k.GetRecord(new PageRecordReference(1, 0)))
                    .Return(new HeapHeader { Fullness = .9, LogicalPageNum = 5 });
            }
            IPage<TestRecord> page;

            using (mocks.Playback())
                page = CreatePage();
            var reference = new PageRecordReference(5,4);
            var tr = new TestRecord { Reference = reference };

            using (mocks.Record())
            {
                physManager.Expect(k => k.RetrievePage(new PageReference(5))).Return(realPage);
                realPage.Expect(k => k.RegisteredPageType).Return(_pageType);
                realPage.Expect(k => k.GetRecord(reference)).Return(tr);
                realPage.Expect(k => k.Dispose());
            }

            using (mocks.Playback())
            {
              Assert.AreEqual(tr, page.GetRecord(reference));
            }

        }

        private void TestHeader(double fullness, uint pageNum)
        {
            var hh = new HeapHeader { Fullness = fullness, LogicalPageNum = pageNum };
            var data = new byte[hh.Size];
            var t = new HeapHeader();
            t.FillBytes(hh, data);
         
            t.FillFromBytes(data, t);
            Assert.AreEqual(fullness,t.Fullness);
            Assert.AreEqual(pageNum, t.LogicalPageNum);
        }

        [TestMethod]
        public void HeapHeaderTest()
        {
            TestHeader(0, 1);
            TestHeader(.5, 10);
            TestHeader(.8, 100000);
            TestHeader(.95,45444);

        }
    }
}
