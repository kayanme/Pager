using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Contracts.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Pager.Pages
{
   [TestClass]
    public class PageBinarySearchTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInit()
        {
            var mp = new MockRepository();
            var headers = mp.StrictMock<IPageHeaders>();
            var access = mp.StrictMock<IRecordAcquirer<TestRecord>>();
            TestContext.Properties.Add("headers", headers);
            TestContext.Properties.Add("access", access);
        }

        private IBinarySearcher<TestRecord> Create()
        {
          

            Headers.Expect(k => k.RecordCount).Return(7).Repeat.Any();
            Headers.Expect(k => k.RecordSize(Arg<ushort>.Is.Anything)).Return(4).Repeat.Any();
            Headers.Replay();
            Access.Replay();
            var page = new BinarySearchContext<TestRecord>(Headers, Access, new PageReference(0), KeyPersistanseType.Logical,()=>{});
          
            return page;
        }

        private IPageHeaders Headers => TestContext.Properties["headers"] as IPageHeaders;
        private IRecordAcquirer<TestRecord> Access => TestContext.Properties["access"] as IRecordAcquirer<TestRecord>;

        [TestMethod]
        public void SearchStartForEmptyPage()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[0]);
            var p = Create();


            Assert.IsNull(p.Current);

        }

        [TestMethod]
        public void SearchStartForPageWithOneRecord()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[] {2});
            Headers.Expect(k => k.RecordShift(2)).Return(10).Repeat.Any();
            var r = new TestRecord { Value = 4 };
            Access.Expect(k => k.GetRecord(10, 4)).Return(r).Repeat.Any();
            var t = Create();
                        
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 2), t.Current.Reference);
            Assert.AreEqual(r, t.Current.Data);

        }

        [TestMethod]
        public void SearchStartForPageWithTwoRecord()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[] {2, 4});
            Headers.Expect(k => k.RecordShift(2)).Return(10).Repeat.Any();
            var r = new TestRecord { Value = 4 };
            Access.Expect(k => k.GetRecord(10, 4)).Return(r).Repeat.Any();
            var t = Create();           
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 2), t.Current.Reference);
            Assert.AreEqual(r, t.Current.Data);

        }

        [TestMethod]
        public void SearchStartForPageWithThreeRecord()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[] {1, 2, 4});
            Headers.Expect(k => k.RecordShift(2)).Return(10).Repeat.Any();
            
            Access.Expect(k => k.GetRecord(10, 4)).Return(new TestRecord { Value = 4 }).Repeat.Any();
            var t = Create();
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 2), t.Current.Reference);
            Assert.IsNotNull(t.Current.Data);

        }

        [TestMethod]
        public void SearchMoveLeftRightForPageWithOneRecord()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[] {2});
            Headers.Expect(k => k.RecordShift(2)).Return(10).Repeat.Any();
            Access.Expect(k => k.GetRecord(10, 4)).Return(new TestRecord { Value = 4 }).Repeat.Any();
            var t = Create();


            Assert.IsFalse(t.MoveLeft());
            Assert.IsFalse(t.MoveRight());
            Assert.IsNotNull(t.Current);

        }

        [TestMethod]
        public void SearchMoveRightLeftForPageWithOneRecord()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[] {2});
            Headers.Expect(k => k.RecordShift(2)).Return(10).Repeat.Any();
            Access.Expect(k => k.GetRecord(10, 4)).Return(new TestRecord { Value = 4 }).Repeat.Any();
            var t = Create();
            Assert.IsFalse(t.MoveRight());
            Assert.IsFalse(t.MoveLeft());
            Assert.IsNotNull(t.Current);

        }

        [TestMethod]
        public void SearchMoveLeftForPageWithTwoRecord()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[] {2, 4});
            Headers.Expect(k => k.RecordShift(2)).Return(10).Repeat.Any();
            Access.Expect(k => k.GetRecord(10, 4)).Return(new TestRecord { Value = 4 }).Repeat.Any();
            var t = Create();


            Assert.IsFalse(t.MoveLeft());
            Assert.IsNotNull(t.Current);

        }


        [TestMethod]
        public void SearchMoveRightForPageWithTwoRecord()
        {
            Headers.Expect(k => k.NonFreeRecords()).Return(new ushort[] {2, 4});
            Headers.Expect(k => k.RecordShift(4)).Return(10).Repeat.Any();
            Access.Expect(k => k.GetRecord(10, 4)).Return(new TestRecord()).Repeat.Any();
            var t = Create();            
            Assert.IsTrue(t.MoveRight());
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 4), t.Current.Reference);

        }
    }
}
