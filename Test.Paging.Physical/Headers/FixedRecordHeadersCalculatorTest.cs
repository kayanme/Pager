using System.IO.Paging.PhysicalLevel.Implementations.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Paging.PhysicalLevel.Headers
{
    [TestClass]
    public class FixedRecordHeadersCalculatorTest
    {
        [TestMethod]
        public void Test1()
        {
            var t = new FixedPageParametersCalculator(108,1);
            t.CalculatePageParameters();
            Assert.AreEqual(96,t.MaxRecordCount);
            Assert.AreEqual(12,t.PamSize);
            Assert.AreEqual(0,t.LastMask);
            t.CalculateUsed(new byte[]{0,0,0,0,0,0,0,0,0,0,0,0});
            Assert.AreEqual(0,t.UsedRecords);
        
        }


        [TestMethod]
        public void Test2()
        {
            var t = new FixedPageParametersCalculator(108, 1);
            t.CalculatePageParameters();
            Assert.AreEqual(96, t.MaxRecordCount);
            Assert.AreEqual(12, t.PamSize);
            Assert.AreEqual(0, t.LastMask);
            t.CalculateUsed(new byte[] { 0, 0, 0, 0x0F,
                                      0, 0, 0, 0,
                                   0xF0, 0, 0, 0 });
            Assert.AreEqual(8, t.UsedRecords);
          
        }

        [TestMethod]
        public void Test3()
        {
            var t = new FixedPageParametersCalculator(128, 1);
            t.CalculatePageParameters();
            Assert.AreEqual(113, t.MaxRecordCount);
            Assert.AreEqual(15, t.PamSize);
            unchecked
            {
                Assert.AreEqual((int)0xFF_FE_00_00, t.LastMask);
            }
            t.CalculateUsed(new byte[] { 0, 0, 0, 0,
                                      0, 0, 0, 0,
                                      0, 0, 0, 0,
                                      0, 0, 0 });//три байта, но под запись в последнем может быть выделен только один бит, т.к. записей 113
            Assert.AreEqual(0, t.UsedRecords);
        
        }

        [TestMethod]
        public void Test4()
        {
            var t = new FixedPageParametersCalculator(128, 1);
            t.CalculatePageParameters();
            Assert.AreEqual(113, t.MaxRecordCount);
            Assert.AreEqual(15, t.PamSize);
            unchecked
            {
                Assert.AreEqual((int) 0xFF_FE_00_00, t.LastMask);
            }
            t.CalculateUsed(new byte[] { 0,    0, 0, 0x0F,
                                      0,    0, 0, 0,
                                   0xF0,    0, 0, 0,
                                   0x0C, 0x0F, 0 });
            Assert.AreEqual(14, t.UsedRecords);
            
        }


        [TestMethod]
        public void Test5()
        {
            var t = new FixedPageParametersCalculator(8192, 8);
            t.CalculatePageParameters();
            Assert.AreEqual(1008, t.MaxRecordCount);
            Assert.AreEqual(126, t.PamSize);
            unchecked
            {
                Assert.AreEqual((int)0xFFFF0000, t.LastMask);
            }
            
        }

        [TestMethod]
        public void Test8()
        {
            var t = new FixedPageParametersCalculator(390, 8);
            t.CalculatePageParameters();
            Assert.AreEqual(48, t.MaxRecordCount);
            Assert.AreEqual(6, t.PamSize);
            unchecked
            {
                Assert.AreEqual((int) 0xFFFF0000, t.LastMask);
            }

            t.CalculateUsed(new byte[]
            {
                0, 0x1,
                0, 0x02,
                0, 0
            });
            Assert.AreEqual(2, t.UsedRecords);
         
        }

        [TestMethod]
        public void Test7()
        {
            var t = new FixedPageParametersCalculator(30, 8,16);
            t.CalculatePageParameters();
            Assert.AreEqual(3, t.MaxRecordCount);
            Assert.AreEqual(6, t.PamSize);
            unchecked
            {
                Assert.AreEqual((int)0xFFFF0000, t.LastMask);
            }
        }

        [TestMethod]
        public void Test9()
        {
            var t = new FixedPageParametersCalculator(0x2000, 7);
            t.CalculatePageParameters();
            Assert.AreEqual(1149, t.MaxRecordCount);
            Assert.AreEqual(144, t.PamSize);
            unchecked
            {
                Assert.AreEqual((int)0xE0_00_00_00, t.LastMask);
            }
        }


        [TestMethod]
        public void Test6()
        {
            var t = new FixedPageParametersCalculator(8192, 8,16);
            t.CalculatePageParameters();
            Assert.AreEqual(819, t.MaxRecordCount);
            Assert.AreEqual(1638, t.PamSize);
            unchecked
            {
                Assert.AreEqual((int)0xFFFF0000, t.LastMask);
            }

        }
    }
}
