using System.Security.AccessControl;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using File.Paging.PhysicalLevel.Implementations.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Pager.Headers
{
    [TestClass]
    public class FixedRecordPageNoOrderHeadersTest
    {

        public TestContext TestContext { get; set; }

        private IPageHeaders Create(int headers,int usedRecords)
        {
            var m = new MockRepository().StrictMock<IPageAccessor>();                  
            m.Replay();

            var calc = new MockRepository().StrictMock<FixedPageParametersCalculator>((ushort)100,(ushort)8, (ushort)1);
            calc.Expect(k => k.FixedRecordSize).Return((ushort)8);
            calc.Expect(k => k.PageAllocationMap).Return(new []{headers});
            calc.Expect(k => k.PamSize).Return(3);
            unchecked
            {
                calc.Expect(k => k.LastMask).Return((int) 0xFF_FC_00_00); //22 records
            }
            calc.Expect(k => k.UsedRecords).Return(usedRecords);
            calc.Replay();
            var p = new FixedRecordPhysicalOnlyHeader(m, calc);
            TestContext.Properties.Add("page", m);
            return p;
        }

        private IPageAccessor Page => TestContext.Properties["page"] as IPageAccessor;

        [TestMethod]
        public void FreePage_ThatNotFree()
        {
            var pageContent = 0b1;
            var headers = Create(pageContent,1);
        
            Page.Expect(k => k.SetByteArray(new byte[] { 0x0,0,0 }, 0, 3));
            Page.Replay();
            headers.FreeRecord(3);
            Assert.AreEqual(0,headers.RecordCount);
            Assert.AreEqual(3, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = 0;
            var headers = Create(pageContent,0);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] { 0,0,0 }, 0, 3));
            Page.Replay();
            headers.FreeRecord(3);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenAvailable()
        {
            int pageContent;
            unchecked
            {
                pageContent = (int)0x00_00_00_01;
            }
            
            var headers = Create(pageContent,1);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] { 0x03,0,0 }, 0, 3));
            Page.Replay();
            var pos = headers.TakeNewRecord(0,8);
            Assert.AreEqual(11, pos);
            Assert.AreEqual(11,headers.RecordShift(11));
            Assert.AreEqual(8, headers.RecordSize(11));
            Assert.AreEqual(2, headers.RecordCount);
            Assert.AreEqual(19, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }

      


        [TestMethod]
        public void AcquirePage_WhenNotAvailable()
        {
            int pageContent;
            unchecked
            {
                pageContent = (int)0x00_03_FF_FF;
            }         
            var headers = Create(pageContent,22);
        
            var pos = headers.TakeNewRecord(0,8);
            Assert.AreEqual(-1, pos);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void IsPageFree_ThatNotFree()
        {
            int pageContent;
            unchecked
            {
                pageContent = (int)0x00_00_00_01;
            }         
            var headers = Create(pageContent,1);
            
            var isFree = headers.IsRecordFree(3);
            Page.VerifyAllExpectations();
            Assert.AreEqual(false,isFree);
        }

        [TestMethod]
        public void IsPageFree_ThatIsFree()
        {

            int pageContent;
            unchecked
            {
                pageContent = (int)0x00_00_00_00;
            }
            var headers = Create(pageContent, 1);

            var isFree = headers.IsRecordFree(3);
            Page.VerifyAllExpectations();
            Assert.AreEqual(isFree, true);
        }
    }
}
