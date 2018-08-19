using System;
using System.Linq;
using FakeItEasy;
using FakeItEasy.Core;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Paging.PhysicalLevel.Headers
{
    [TestClass]
    public class FixedRecordPageNoOrderHeadersTest
    {

        public TestContext TestContext { get; set; }

        private IPageHeaders Create(int headers, int usedRecords)
        {
            var m = A.Fake<IPageAccessor>();


            var calc = A.Fake<FixedPageParametersCalculator>(o => o.WithArgumentsForConstructor(new[] { (object)(ushort)100, (ushort)8, (ushort)1 }));
            A.CallTo(() => calc.FixedRecordSize).Returns((ushort)8);
            A.CallTo(() => calc.PamSize).Returns(3);
            A.CallTo(() => calc.PamIntLength).Returns(1);
            A.CallTo(() => calc.BitsUnusedInLastInt).Returns<byte>(14);
            unchecked
            {
                A.CallTo(() => calc.LastMask).Returns((int)0xFF_FC_00_00); //22 records
            }
            A.CallTo(() => calc.UsedRecords).Returns(usedRecords);

            var p = new FixedRecordPhysicalOnlyHeader(m, calc, usedRecords);
            TestContext.Properties.Add("page", m);
            return p;
        }

        private IPageAccessor Page => TestContext.Properties["page"] as IPageAccessor;

        private Action<IFakeObjectCall> VerifyForArray(byte[] sourceAr,byte[] targetArr)
        {
            unsafe
            {
                return
                    a =>
                    {                        
                        fixed (byte* t2 = sourceAr)
                        {
                            ((ByteAction)a.Arguments[2])(t2);
                        }
                        CollectionAssert.AreEqual(sourceAr.Take(targetArr.Length).ToArray(), targetArr);
                    };
            }
        }

        private unsafe void MakeAccessorExpectation(int position, int length, byte[] sourceAr, byte[] targetArr)
        {
            A.CallTo(() => Page.QueueByteArrayOperation((position), (length), A<ByteAction>.Ignored))
                              .Invokes(VerifyForArray(sourceAr, targetArr));
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

            headers.FreeRecord(3);
            Assert.AreEqual(0,headers.RecordCount);
            Assert.AreEqual(3, headers.TotalUsedSize);

        }

        [TestMethod]
        public void FreePage_ThatIsFree()
        {
            var pageContent = 0;
            var headers = Create(pageContent,0);

            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] { 0x0, 0, 0 });            

            headers.FreeRecord(3);

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

            MakeAccessorExpectation(0, 3, fromPageContent(ref pageContent), new byte[] { 0x03, 0, 0 });            

            var pos = headers.TakeNewRecord(0,8);
            Assert.AreEqual(11, pos);
            Assert.AreEqual(11,headers.RecordShift(11));
            Assert.AreEqual(8, headers.RecordSize(11));
            Assert.AreEqual(2, headers.RecordCount);
            Assert.AreEqual(19, headers.TotalUsedSize);

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

            Assert.AreEqual(isFree, true);
        }
    }
}
