using System;
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
    public class FixedRecordPageNoOrderHeadersTest
    {

        public TestContext TestContext { get; set; }

        private IPageHeaders Create(int headers,int usedRecords)
        {
            var m = new MockRepository().StrictMock<IPageAccessor>();                  
            m.Replay();

            var calc = new MockRepository().StrictMock<FixedPageParametersCalculator>((ushort)100,(ushort)8, (ushort)1);
            calc.Expect(k => k.FixedRecordSize).Return((ushort)8);           
            calc.Expect(k => k.PamSize).Return(3).Repeat.Any();
            calc.Expect(k => k.PamIntLength).Return(1).Repeat.Any();
            calc.Expect(k => k.BitsUnusedInLastInt).Return(14).Repeat.Any();
            unchecked
            {
                calc.Expect(k => k.LastMask).Return((int) 0xFF_FC_00_00); //22 records
            }
            calc.Expect(k => k.UsedRecords).Return(usedRecords);
            calc.Replay();
            var p = new FixedRecordPhysicalOnlyHeader(m, calc, usedRecords);
            TestContext.Properties.Add("page", m);
            return p;
        }

        private IPageAccessor Page => TestContext.Properties["page"] as IPageAccessor;

        private Action<int, int, ByteAction> VerifyForArray(byte[] sourceAr,byte[] targetArr)
        {
            unsafe
            {
                return
                    (_, s, d) =>
                    {                        
                        fixed (byte* t2 = sourceAr)
                        {
                            d(t2);
                        }
                        CollectionAssert.AreEqual(sourceAr.Take(targetArr.Length).ToArray(), targetArr);
                    };
            }
        }

        private unsafe void MakeAccessorExpectation(int position,int length, byte[] sourceAr, byte[] targetArr)
        {
            Page.Expect(k => k.QueueByteArrayOperation(Arg.Is(position),Arg.Is(length),Arg<ByteAction>.Is.Anything))
                .Do(VerifyForArray(sourceAr,targetArr));                
        }

        private unsafe byte[] fromPageContent(ref int headers)
        {
            var bytes = new byte[4];
            fixed (int* src = &headers)
            fixed (void* dst = bytes)
            {
                Buffer.MemoryCopy(src, dst, 4, 4);
            }
            return bytes;
        }

        [TestMethod]
        public void FreePage_ThatNotFree()
        {
            var pageContent = 0b1;            
            var headers = Create(pageContent,1);

            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] {0x0, 0, 0});            
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
            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] { 0x0, 0, 0 });            
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
            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] { 0x03, 0, 0 });            
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
            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] { 0xFF, 0xFF, 0x03 });
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
            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] { 0x01, 0x00, 0x00 });
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
            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] { 0x00, 0x00, 0x00 });
            var isFree = headers.IsRecordFree(3);
            Page.VerifyAllExpectations();
            Assert.AreEqual(isFree, true);
        }
    }
}
