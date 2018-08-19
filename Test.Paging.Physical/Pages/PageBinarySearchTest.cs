using FakeItEasy;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace Test.Paging.PhysicalLevel.Pages
{
    [TestClass]
    public class PageBinarySearchTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInit()
        {
          
            var headers = A.Fake<IPageHeaders>();
            var access = A.Fake<IRecordAcquirer<TestRecord>>();
            TestContext.Properties.Add("headers", headers);
            TestContext.Properties.Add("access", access);
        }

        private IBinarySearcher<TestRecord> Create()
        {          
            A.CallTo(()=> Headers.RecordCount).Returns<ushort>(7);
            A.CallTo(() => Headers.RecordSize(A<ushort>.Ignored)).Returns<ushort>(4);
           
            var page = new BinarySearchContext<TestRecord>(Headers, Access, new PageReference(0), KeyPersistanseType.Logical,()=>{});
          
            return page;
        }

        private IPageHeaders Headers => TestContext.Properties["headers"] as IPageHeaders;
        private IRecordAcquirer<TestRecord> Access => TestContext.Properties["access"] as IRecordAcquirer<TestRecord>;

        [TestMethod]
        public void SearchStartForEmptyPage()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[0]);
            var p = Create();


            Assert.IsNull(p.Current);

        }

        [TestMethod]
        public void SearchStartForPageWithOneRecord()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[] {2});
            A.CallTo(() => Headers.RecordShift(2)).Returns<ushort>(10);
            var r = new TestRecord { Value = 4 };
            A.CallTo(() => Access.GetRecord(10, 4)).Returns(r);
            var t = Create();
                        
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 2), t.Current.Reference);
            Assert.AreEqual(r, t.Current.Data);

        }

        [TestMethod]
        public void SearchStartForPageWithTwoRecord()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[] {2, 4});
            A.CallTo(() => Headers.RecordShift(2)).Returns<ushort>(10);
            var r = new TestRecord { Value = 4 };
            A.CallTo(() => Access.GetRecord(10, 4)).Returns(r);
            var t = Create();           
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 2), t.Current.Reference);
            Assert.AreEqual(r, t.Current.Data);

        }

        [TestMethod]
        public void SearchStartForPageWithThreeRecord()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[] {1, 2, 4});
            A.CallTo(() => Headers.RecordShift(2)).Returns<ushort>(10);

            A.CallTo(() => Access.GetRecord(10, 4)).Returns(new TestRecord { Value = 4 });
            var t = Create();
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 2), t.Current.Reference);
            Assert.IsNotNull(t.Current.Data);

        }

        [TestMethod]
        public void SearchMoveLeftRightForPageWithOneRecord()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[] {2});
            A.CallTo(() => Headers.RecordShift(2)).Returns<ushort>(10);
            A.CallTo(() => Access.GetRecord(10, 4)).Returns(new TestRecord { Value = 4 });
            var t = Create();


            Assert.IsFalse(t.MoveLeft());
            Assert.IsFalse(t.MoveRight());
            Assert.IsNotNull(t.Current);

        }

        [TestMethod]
        public void SearchMoveRightLeftForPageWithOneRecord()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[] {2});
            A.CallTo(() => Headers.RecordShift(2)).Returns<ushort>(10);
            A.CallTo(() => Access.GetRecord(10, 4)).Returns(new TestRecord { Value = 4 });
            var t = Create();
            Assert.IsFalse(t.MoveRight());
            Assert.IsFalse(t.MoveLeft());
            Assert.IsNotNull(t.Current);

        }

        [TestMethod]
        public void SearchMoveLeftForPageWithTwoRecord()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[] {2, 4});
            A.CallTo(() => Headers.RecordShift(2)).Returns<ushort>(10);
            A.CallTo(() => Access.GetRecord(10, 4)).Returns(new TestRecord { Value = 4 });
            var t = Create();


            Assert.IsFalse(t.MoveLeft());
            Assert.IsNotNull(t.Current);

        }


        [TestMethod]
        public void SearchMoveRightForPageWithTwoRecord()
        {
            A.CallTo(() => Headers.NonFreeRecords()).Returns(new ushort[] {2, 4});
            A.CallTo(() => Headers.RecordShift(4)).Returns<ushort>(10);
            A.CallTo(() => Access.GetRecord(10, 4)).Returns(new TestRecord());
            var t = Create();            
            Assert.IsTrue(t.MoveRight());
            Assert.AreEqual(new LogicalPositionPersistentPageRecordReference(0, 4), t.Current.Reference);

        }
    }
}
