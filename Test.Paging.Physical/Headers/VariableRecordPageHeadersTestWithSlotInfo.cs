using FakeItEasy;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Implementations.Headers;

namespace Test.Paging.PhysicalLevel.Headers
{
    [TestClass]
    public class VariableRecordPageHeadersTestWithSlotInfo
    {
        public TestContext TestContext { get; set; }

        private IPageHeaders Create(byte[] page)
        {
            var m = A.Fake<IPageAccessor>();
            A.CallTo(() => m.GetByteArray(0, page.Length)).Returns(page);
            A.CallTo(() => m.PageSize).Returns(page.Length);

            var p = new VariableRecordWithLogicalOrderHeaders(m);
            TestContext.Properties.Add("page", m);
            return p;
        }
        private IPageAccessor Page => TestContext.Properties["page"] as IPageAccessor;

        [TestMethod]
        public void FreePage_ThatNotFree()
        {
            var pageContent = new byte[] { 0x10, 0x02, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            

            headers.FreeRecord(0);
            A.CallTo(() => Page.SetByteArray(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0 }), 0, 1)).MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            //A.CallTo(()=>        //    page.SetByteArray(new byte[] { 0 }, 0, 1));

            headers.FreeRecord(0);

        }


        [TestMethod]
        public void AcquirePage_WhenAvailable()
        {
            var pageContent = new byte[] { 0x10, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            

            var pos = headers.TakeNewRecord(1, 3);
            A.CallTo(() => Page.SetByteArray(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x10, 0x03, 0x0, 0x01 }), 6, 4)).MustHaveHappenedOnceExactly();
            Assert.AreEqual(1, pos);
            Assert.AreEqual(10, headers.RecordShift(1));
            Assert.AreEqual(3, headers.RecordSize(1));

        }


        [TestMethod]
        public void AcquirePage_WhenNotAvailable()
        {
            var pageContent = new byte[] { 0x10, 0x06, 0, 0, 0, 0, 0, 0, 0x10, 0x06, 0, 0x01, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var pos = headers.TakeNewRecord(1, 7);
            Assert.AreEqual(-1, pos);

        }

        [TestMethod]
        public void AcquirePage_WhenSizeIsnotEnough()
        {
            var pageContent = new byte[] { 0x10, 0x04, 0, 0, 0, 0, 0, 0, 0x10, 0x03, 0, 0x01, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var pos = headers.TakeNewRecord(1, 7);
            Assert.AreEqual(-1, pos);

        }


        [TestMethod]
        public void IsPageFree_ThatNotFree()
        {
            var pageContent = new byte[] { 0x10, 0x07, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var isFree = headers.IsRecordFree(0);

            Assert.AreEqual(false, isFree);
        }

        [TestMethod]
        public void IsPageFree_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var isFree = headers.IsRecordFree(0);

            Assert.AreEqual(true, isFree);
        }


        [TestMethod]
        public void InitialRead()
        {
            var pageContent = new byte[] { 0x10, 0x02, 0, 0x01, 0, 0, 0x20, 0x03, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);



            Assert.AreEqual(10, headers.RecordShift(0));
            Assert.AreEqual(3, headers.RecordSize(0));
            Assert.AreEqual(2, headers.RecordType(0));

            Assert.AreEqual(4, headers.RecordShift(1));
            Assert.AreEqual(2, headers.RecordSize(1));
            Assert.AreEqual(1, headers.RecordType(1));

        }

        //[TestMethod]
        public void SwapRecords()
        {
            var pageContent = new byte[] { 0x10, 0x02, 0, 0x01, 0, 0, 0x20, 0x03, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
            
            headers.ApplyOrder(new ushort[] { 1, 0 });

            A.CallTo(() => Page.SetByteArray(new byte[] { 0, 0 }, 2, 2)).MustHaveHappenedOnceExactly();
            A.CallTo(() => Page.SetByteArray(new byte[] { 0, 0x01 }, 8, 2)).MustHaveHappenedOnceExactly();

            Assert.AreEqual(10, headers.RecordShift(1));
            Assert.AreEqual(3, headers.RecordSize(1));
            Assert.AreEqual(2, headers.RecordType(1));

            Assert.AreEqual(4, headers.RecordShift(0));
            Assert.AreEqual(2, headers.RecordSize(0));
            Assert.AreEqual(1, headers.RecordType(0));
        }
    }
}
