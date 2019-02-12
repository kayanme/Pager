using FakeItEasy;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Implementations.Headers;

namespace Test.Paging.PhysicalLevel.Headers
{
    [TestClass]
    public class VariableRecordPageHeadersTest
    {
        public TestContext TestContext { get; set; }

        private IPageHeaders Create(byte[] page)
        {


            var m = A.Fake<IPageAccessor>();

            A.CallTo(() => m.GetByteArray(0, page.Length)).Returns(page);
            A.CallTo(() => m.PageSize).Returns(page.Length);

            var p = new VariableRecordPageHeaders(m);
            TestContext.Properties.Add("page", m);

            return p;
        }
        private IPageAccessor Page => TestContext.Properties["page"] as IPageAccessor;

        [TestMethod]
        public void FreeRecord_ThatNotFree()
        {
            var pageContent = new byte[] { 0x10, 0x02, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            headers.FreeRecord(0);
            A.CallTo(() => Page.SetByteArray(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0 }), 0, 1)).MustHaveHappened();
        }

        [TestMethod]
        public void FreeRecord_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            //A.CallTo(()=>         //   page.SetByteArray(new byte[] { 0 }, 0 + shift, 1));

            headers.FreeRecord(0);

        }


        [TestMethod]
        public void AcquireRecord_WhenAvailable()
        {
            var pageContent = new byte[] { 0x10, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
            var pos = headers.TakeNewRecord(3);

            A.CallTo(() => Page.SetByteArray(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x03 }), 4, 2)).MustHaveHappened();

            Assert.AreEqual(1, pos);
            Assert.AreEqual(6, headers.RecordShift(1));
            Assert.AreEqual(3, headers.RecordSize(1));

            Assert.IsFalse(headers.IsRecordFree((ushort)pos));

        }


        [TestMethod]
        public void AcquireRecord_WhenNotAvailable()
        {
            var pageContent = new byte[] { 0x10, 0x06, 0, 0, 0, 0, 0, 0, 0x10, 0x06, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var pos = headers.TakeNewRecord( 7);
            Assert.AreEqual(-1, pos);

        }

        [TestMethod]
        public void AcquireRecord_WhenSizeIsnotEnough()
        {
            var pageContent = new byte[] { 0x10, 0x06, 0, 0, 0, 0, 0, 0, 0x10, 0x05, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var pos = headers.TakeNewRecord( 7);
            Assert.AreEqual(-1, pos);

        }


        [TestMethod]
        public void IsRecordFree_ThatNotFree()
        {
            var pageContent = new byte[] { 0x10, 0x07, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var isFree = headers.IsRecordFree(0);

            Assert.IsFalse(isFree);
        }

        [TestMethod]
        public void IsRecordFree_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);

            var isFree = headers.IsRecordFree(0);

            Assert.AreEqual(isFree, true);
        }


        [TestMethod]
        public void InitialRead()
        {
            var pageContent = new byte[] { 0x10, 0x02, 0, 0, 0x20, 0x03, 0, 0, 0, 0 };
            var headers = Create(pageContent);



            Assert.AreEqual(2, headers.RecordShift(0));
            Assert.AreEqual(2, headers.RecordSize(0));
            
            Assert.AreEqual(6, headers.RecordShift(1));
            Assert.AreEqual(3, headers.RecordSize(1));
            
            Assert.AreEqual(2, headers.RecordCount);
            Assert.AreEqual(9, headers.TotalUsedSize);
        }
    }
}
