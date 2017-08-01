using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pager;
using Pager.Contracts;
using Pager.Implementations;
using Rhino.Mocks;

namespace Test.Pager
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
        private IPageAccessor page => TestContext.Properties["page"] as IPageAccessor;

        [TestMethod]
        public void FreePage_ThatNotFree()
        {
            var pageContent = new byte[] { 0x80, 0,0,0,0,0,0,0 };
            var headers = Create(pageContent);
            page.BackToRecord();
            page.Expect(k => k.SetByteArray(new byte[] { 0 }, 0, 1));
            page.Replay();
            headers.FreeRecord(0);
            page.VerifyAllExpectations();
        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
            page.BackToRecord();
            page.Expect(k => k.SetByteArray(new byte[] { 0 }, 0, 1));
            page.Replay();
            headers.FreeRecord(0);
            page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenAvailable()
        {
            var pageContent = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0,   0,0,0,0,0,0,0,0 };
            var headers = Create(pageContent);
            page.BackToRecord();
            page.Expect(k => k.SetByteArray(new byte[] { 0x80 }, 8, 1));
            page.Replay();
            var pos = headers.TakeNewRecord(0,7);
            Assert.AreEqual(1, pos);
            Assert.AreEqual(10,headers.RecordShift(1));
            Assert.AreEqual(7, headers.RecordSize(1));            
            page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenNotAvailable()
        {
            var pageContent = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0, 0x80, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
        
            var pos = headers.TakeNewRecord(0,7);
            Assert.AreEqual(-1, pos);
            page.VerifyAllExpectations();
        }


        [TestMethod]
        public void IsPageFree_ThatNotFree()
        {
            var pageContent = new byte[] { 0x80, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
            
            var isFree = headers.IsRecordFree(0);
            page.VerifyAllExpectations();
            Assert.AreEqual(isFree, false);
        }

        [TestMethod]
        public void IsPageFree_ThatIsFree()
        {
            var pageContent = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var headers = Create(pageContent);
          
            var isFree = headers.IsRecordFree(0);
            page.VerifyAllExpectations();
            Assert.AreEqual(isFree, true);
        }
    }
}
