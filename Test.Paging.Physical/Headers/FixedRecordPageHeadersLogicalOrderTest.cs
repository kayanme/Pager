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
    public class FixedRecordPageLogicalOrderHeadersTest
    {

        public TestContext TestContext { get; set; }

        private unsafe IPageHeaders Create(long headers,int usedRecords)
        {
            var m = new MockRepository().StrictMock<IPageAccessor>();                  
         

            var calc = new MockRepository().StrictMock<FixedPageParametersCalculator>((ushort)100,(ushort)8, (ushort)16);
            calc.Expect(k => k.FixedRecordSize).Return((ushort)8).Repeat.Any();
            calc.Expect(k => k.PamIntLength).Return((ushort)2).Repeat.Any();
            
            int[] header;
            unchecked
            {
                header = new[]
                    {(int) ((headers & (long) 0xFF_FF_FF_FF_00_00_00_00) >> 32),
                    (int) (headers & (int) 0xFF_FF_FF_FF)};
                
            }
          
                
            calc.Expect(k => k.PamSize).Return(6).Repeat.Any();
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
        private unsafe byte[] fromPageContent(ref long headers)
        {
            var bytes = new byte[6];
            fixed (long* src = &headers)
            fixed (void* dst = bytes)
            {
                Buffer.MemoryCopy(src, dst, 6,6);
            }
            return bytes;
        }
        private Action<int, int, ByteAction> VerifyForArray(byte[] sourceAr, byte[] targetArr)
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

        private unsafe void MakeAccessorExpectation(int position, byte[] sourceAr, byte[] targetArr)
        {
            Page.Expect(k => k.QueueByteArrayOperation(Arg.Is(position), Arg.Is(6), Arg<ByteAction>.Is.Anything))
                .Do(VerifyForArray(sourceAr, targetArr));
            Page.Replay();
        }

        [TestMethod]
        public void FreePage_ThatNotFree()
        {
            long pageContent;
            unchecked
            {
                pageContent = 0x00_00_00_00_00_00_01_00;
            }
            
            var headers = Create(pageContent,1);
            MakeAccessorExpectation(0,fromPageContent(ref pageContent), new byte[] { 0x0, 0, 0, 0, 0, 0 });          
            headers.FreeRecord(6);
            Assert.AreEqual(0,headers.RecordCount);
            Assert.AreEqual(6, headers.TotalUsedSize);
            Page.VerifyAllExpectations();
        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = 0L;
            var headers = Create(pageContent,0);

            MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x0, 0, 0, 0, 0, 0 });
            headers.FreeRecord(6);
            Page.VerifyAllExpectations();
        }


        [TestMethod]
        public void AcquirePage_WhenAvailable()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_00_00_00_00_01_00;
            }
            
            var headers = Create(pageContent,1);
            MakeAccessorExpectation(0,  fromPageContent(ref pageContent), new byte[] { 0, 0x01, 0xFF, 0xFF, 0, 0 });          
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
                pageContent = (long)0x00_00_00_00_FF_FF_FF_FF;
            }

            var headers = Create(pageContent, 2);
            MakeAccessorExpectation(0,  fromPageContent(ref pageContent), new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            
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
                pageContent = (long)0x00_00_03_00_02_00_02_00;
            }         
            var headers = Create(pageContent,3);
            MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x02, 0x00, 0x02, 0x00, 0x03 });
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
                pageContent = (long)0x00_00_FF_FF_01_00_02_00;
            }
            var headers = Create(pageContent, 3);
            MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x02, 0x00, 0x01, 0xFF, 0xFF });
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
                pageContent = (long)0x00_00_00_00_00_00_02_00;
            }         
            var headers = Create(pageContent,1);
            MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x02, 0x00, 0x00, 0x00, 0x00 });
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
                pageContent = (long)0x00_00_FF_FF_FF_FF_FF_FF;
            }
            var headers = Create(pageContent, 3);
            MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x02, 0x00, 0x01, 0x00, 0x03 });
            headers.ApplyOrder(new ushort[]{14,6,22});
            Page.VerifyAllExpectations();            
        }

        [TestMethod]
        public void IsPageFree_ThatIsFree()
        {

            long pageContent;
            unchecked
            {
                pageContent = 0x00_00_00_00;
            }
            var headers = Create(pageContent, 0);
            MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            var isFree = headers.IsRecordFree(6);
            Page.VerifyAllExpectations();
            Assert.AreEqual(isFree, true);
        }
    }
}
