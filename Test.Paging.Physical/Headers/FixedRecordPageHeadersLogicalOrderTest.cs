using System.Linq;
using System.Security.AccessControl;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using File.Paging.PhysicalLevel.Implementations.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Pager.Headers
{
    [TestClass]
    public class FixedRecordPageLogicalOrderHeadersTest
    {

        public TestContext TestContext { get; set; }

        private IPageHeaders Create(long headers,int usedRecords)
        {
            var m = new MockRepository().StrictMock<IPageAccessor>();                  
            m.Replay();

            var calc = new MockRepository().StrictMock<FixedPageParametersCalculator>((ushort)100,(ushort)8, (ushort)16);
            calc.Expect(k => k.FixedRecordSize).Return((ushort)8);
            unchecked
            {
                calc.Expect(k => k.PageAllocationMap).Return(new[] {(int)( (headers & (long)0xFF_FF_FF_FF_00_00_00_00) >> 32), (int)( headers & (int)0xFF_FF_FF_FF) });
            }
            
            calc.Expect(k => k.PamSize).Return(6);
            unchecked
            {
                calc.Expect(k => k.LastMask).Return((int) 0xFF_FF_00_00); //3 records
            }
            calc.Expect(k => k.UsedRecords).Return(usedRecords);
            calc.Replay();
            var p = new FixedRecordWithLogicalOrderHeader(m, calc);
            TestContext.Properties.Add("page", m);
            return p;
        }

        private IPageAccessor Page => TestContext.Properties["page"] as IPageAccessor;

        [TestMethod]
        public void FreePage_ThatNotFree()
        {
            long pageContent;
            unchecked
            {
                pageContent = 0x00_00_01_00_00_00_00_00;
            }
            
            var headers = Create(pageContent,1);
        
            Page.Expect(k => k.SetByteArray(new byte[] { 0x0,0,0,0,0,0 }, 0, 6));
            Page.Replay();
            headers.FreeRecord(6);
            Assert.AreEqual(0,headers.RecordCount);
            Assert.AreEqual(6, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = 0;
            var headers = Create(pageContent,0);
            
            Page.Replay();
            headers.FreeRecord(6);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenAvailable()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_01_00_00_00_00_00;
            }
            
            var headers = Create(pageContent,1);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] {0,0x01, 0xFF, 0xFF, 0,0 }, 0, 6));
            Page.Replay();
            var pos = headers.TakeNewRecord(0,8);
            Assert.AreEqual(14, pos);
            Assert.AreEqual(14,headers.RecordShift(14));
            Assert.AreEqual(8, headers.RecordSize(14));
            Assert.AreEqual(2, headers.RecordCount);
            Assert.AreEqual(22, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }

        [TestMethod]
        public void AcquirePage_WhenAvailable2()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0xFF_FF_FF_FF_00_00_00_00;
            }

            var headers = Create(pageContent, 2);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0, 6));
            Page.Replay();
            var pos = headers.TakeNewRecord(0, 8);
            Assert.AreEqual(22, pos);
            Assert.AreEqual(22, headers.RecordShift(22));
            Assert.AreEqual(8, headers.RecordSize(22));
            Assert.AreEqual(3, headers.RecordCount);
            Assert.AreEqual(30, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenNotAvailable()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x02_00_02_00_00_00_03_00;
            }         
            var headers = Create(pageContent,3);
        
            var pos = headers.TakeNewRecord(0,8);
            Assert.AreEqual(-1, pos);
            Page.VerifyAllExpectations();
        }

        [TestMethod]
        public void Iterate_Pages()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x01_00_02_00_00_00_FF_FF;
            }
            var headers = Create(pageContent, 3);

            var records = headers.NonFreeRecords().ToArray();
            CollectionAssert.AreEqual(new ushort[]{14,6,22},records);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void IsPageFree_ThatNotFree()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_02_00_00_00_00_00;
            }         
            var headers = Create(pageContent,1);
            
            var isFree = headers.IsRecordFree(6);
            Page.VerifyAllExpectations();
            Assert.AreEqual(false,isFree);
        }

        [TestMethod]
        public void PageApplyOrder()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0xFF_FF_FF_FF_00_00_FF_FF;
            }
            var headers = Create(pageContent, 3);
            Page.BackToRecord();
            Page.Expect(k => k.SetByteArray(new byte[] { 0x0, 0x2, 0, 0x1, 0, 0x3 }, 0, 6));
            Page.Replay();
            headers.ApplyOrder(new ushort[]{14,6,22});
            Page.VerifyAllExpectations();            
        }

        [TestMethod]
        public void IsPageFree_ThatIsFree()
        {

            int pageContent;
            unchecked
            {
                pageContent = (int)0x00_00_00_00;
            }
            var headers = Create(pageContent, 0);

            var isFree = headers.IsRecordFree(6);
            Page.VerifyAllExpectations();
            Assert.AreEqual(isFree, true);
        }
    }
}
