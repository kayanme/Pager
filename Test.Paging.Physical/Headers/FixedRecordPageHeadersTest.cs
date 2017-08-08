using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Pager.Headers
{
    [TestClass]
    public class FixedRecordPageHeadersTest
    {

        public TestContext TestContext { get; set; }

        private IPageHeaders Create(byte[] page)
        {
            var m = new MockRepository().StrictMock<IPageAccessor>();

            m.Expect(k => k.GetByteArray(0, page.Length)).Return(page);
            m.Expect(k => k.PageSize).Repeat.Any().Return(page.Length);
            m.Replay();
            var p = new FixedRecordPageHeaders(m, 7);
            TestContext.Properties.Add("page", m);
            return p;
        }
        private IPageAccessor Page => TestContext.Properties["page"] as IPageAccessor;

        [TestMethod]
        public void FreePage_ThatNotFree()
        {
            var pageContent = new byte[] { 0x80, 0,0,0,0,0,0,0 };
            var headers = Create(pageContent);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] { 0 }, 0, 1));
            Page.Replay();
            headers.FreeRecord(0);
            Assert.AreEqual(0,headers.RecordCount);
            Assert.AreEqual(0, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] { 0 }, 0, 1));
            Page.Replay();
            headers.FreeRecord(0);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenAvailable()
        {
            var pageContent = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0,   0,0,0,0,0,0,0,0 };
            var headers = Create(pageContent);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] { 0x80 }, 8, 1));
            Page.Replay();
            var pos = headers.TakeNewRecord(0,7);
            Assert.AreEqual(1, pos);
            Assert.AreEqual(10,headers.RecordShift(1));
            Assert.AreEqual(7, headers.RecordSize(1));
            Assert.AreEqual(2, headers.RecordCount);
            Assert.AreEqual(14, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenNotAvailable()
        {
            var pageContent = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0, 0x80, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
        
            var pos = headers.TakeNewRecord(0,7);
            Assert.AreEqual(-1, pos);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void IsPageFree_ThatNotFree()
        {
            var pageContent = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
            
            var isFree = headers.IsRecordFree(0);
            Page.VerifyAllExpectations();
            Assert.AreEqual(isFree, false);
        }

        [TestMethod]
        public void IsPageFree_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
          
            var isFree = headers.IsRecordFree(0);
            Page.VerifyAllExpectations();
            Assert.AreEqual(isFree, true);
        }
    }
}
