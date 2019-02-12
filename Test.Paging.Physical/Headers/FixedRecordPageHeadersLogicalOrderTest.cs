using System;
using System.Linq;
using FakeItEasy;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;

namespace Test.Paging.PhysicalLevel.Headers
{
    [TestClass]
    public class FixedRecordPageLogicalOrderHeadersTest
    {

        public TestContext TestContext { get; set; }

        private unsafe IPageHeaders Create(long headers, int usedRecords)
        {
            var m = A.Fake<IPageAccessor>();
            var calc = A.Fake<FixedPageParametersCalculator>(o => o.WithArgumentsForConstructor(new[] { (object)(ushort)100, (ushort)8, (ushort)16 }));
            A.CallTo(() => calc.FixedRecordSize).Returns((ushort)8);
            A.CallTo(() => calc.PamIntLength).Returns((ushort)2);

            int[] header;
            unchecked
            {
                header = new[]
                    {(int) ((headers & (long) 0xFF_FF_FF_FF_00_00_00_00) >> 32),
                    (int) (headers & (int) 0xFF_FF_FF_FF)};

            }


            A.CallTo(() => calc.PamSize).Returns(6);
            unchecked
            {
                A.CallTo(() => calc.LastMask).Returns((int)0xFF_FF_00_00); //3 records
            }
            A.CallTo(() => calc.UsedRecords).Returns(usedRecords);

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

        private unsafe Action MakeAccessorExpectation(int position, byte[] sourceAr, byte[] targetArr)
        {
            A.CallTo(() => Page.QueueByteArrayOperation((position), (6), A<ByteAction>.Ignored))
                .Invokes(a => VerifyForArray(sourceAr, targetArr)((int)a.Arguments[0], (int)a.Arguments[1],(ByteAction)a.Arguments[2])); ;
            return ()=>A.CallTo(() => Page.QueueByteArrayOperation((position), (6), A<ByteAction>.Ignored)).MustHaveHappened();
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

        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = 0L;
            var headers = Create(pageContent,0);

            MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x0, 0, 0, 0, 0, 0 });
            headers.FreeRecord(6);

        }


        [TestMethod]
        public void AcquirePage_WhenAvailable_HasRecordWithOrder()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_00_00_00_00_01_00;
            }
            
            var headers = Create(pageContent,1);
            MakeAccessorExpectation(0,fromPageContent(ref pageContent), new byte[] { 0, 0x01, 0xFF, 0xFF, 0, 0 });          
            var pos = headers.TakeNewRecord(8);
            Assert.AreEqual(14, pos);
            Assert.AreEqual(14,headers.RecordShift(14));
            Assert.AreEqual(8, headers.RecordSize(14));
            Assert.AreEqual(2, headers.RecordCount);
            Assert.AreEqual(22, headers.TotalUsedSize);

        }

        [TestMethod]
        public void AcquirePage_WhenAvailable_HasRecordsWithoutOrder()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_00_00_FF_FF_FF_FF;
            }

            var headers = Create(pageContent, 2);
            MakeAccessorExpectation(0,  fromPageContent(ref pageContent), new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            
            var pos = headers.TakeNewRecord(8);
            Assert.AreEqual(22, pos);
            Assert.AreEqual(22, headers.RecordShift(22));
            Assert.AreEqual(8, headers.RecordSize(22));
            Assert.AreEqual(3, headers.RecordCount);
            Assert.AreEqual(30, headers.TotalUsedSize);

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
            var pos = headers.TakeNewRecord(8);
            Assert.AreEqual(-1, pos);

        }

        [TestMethod]
        public void Iterate_Pages_WhenAbsentOrderIsLast()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_FF_FF_01_00_00_00;
            }
            var headers = Create(pageContent, 3);
            var check = MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF });
            var records = headers.NonFreeRecords().ToArray();
            CollectionAssert.AreEqual(new ushort[]{22,14},records);
            //uknown order is always first
            check();
        }

        [TestMethod]
        public void Iterate_Pages_WhenAbsentOrderIsFirst()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_FF_FF_02_00_00_00;
            }
            var headers = Create(pageContent, 3);
            var check = MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF });
            var records = headers.NonFreeRecords().ToArray();
            CollectionAssert.AreEqual(new ushort[] { 22, 14 }, records);
            
            check();
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
            var check = MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x02, 0x00, 0x01, 0x00, 0x03 });
            headers.ApplyOrder(new ushort[]{14,6,22});
            var records = headers.NonFreeRecords().ToArray();
            CollectionAssert.AreEqual(new ushort[] { 14, 6, 22 }, records);

            check();
        }

        [TestMethod]
        public void PageDropOrder()
        {
            long pageContent;
            unchecked
            {
                pageContent = (long)0x00_00_02_00_01_00_03_00;
            }
            var headers = Create(pageContent, 3);
            var check = MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x03, 0x00, 0x01, 0xFF, 0xFF });
            headers.DropOrder(22);

            var records = headers.NonFreeRecords().ToArray();
            CollectionAssert.AreEqual(new ushort[] {22, 14, 6 }, records);
            
            check();
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
            var check = MakeAccessorExpectation(0, fromPageContent(ref pageContent), new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            var isFree = headers.IsRecordFree(6);
            check();
            Assert.AreEqual(isFree, true);
        }
    }
}
