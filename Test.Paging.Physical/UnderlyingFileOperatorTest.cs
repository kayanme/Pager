using System.IO;
using File.Paging.PhysicalLevel;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Paging.PhysicalLevel
{
    /// <summary>
    /// Summary description for UnderlyingFileOperatorTest
    /// </summary>
    [TestClass]
    public class UnderlyingFileOperatorTest
    {
       

     

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private string FileName => TestContext.TestName;
        private FileStream _file;
        private IUnderlyingFileOperator GetOperator(int initSize)
        {
            _file = System.IO.File.Open(FileName, FileMode.OpenOrCreate);
            _file.SetLength(initSize);
            _file.Flush();
            return new UnderyingFileOperator(_file);
        }

        [TestCleanup]
        public void Clean()
        {
          
            try
            {
                System.IO.File.Delete(FileName);
            }
            catch { }
        }

        [TestMethod]
        public void TakeMap_OneFileIsSmaller()
        {
            var opr = GetOperator(Extent.Size);
            var map = opr.GetMappedFile(Extent.Size*2);
            opr.Dispose();
            map.Dispose();            
            _file.Dispose();
            var f = System.IO.File.Open(FileName, FileMode.OpenOrCreate);
            Assert.AreEqual(Extent.Size * 2, f.Length);
            f.Dispose();
        }

        [TestMethod]
        public void TakeMap()
        {
            var opr = GetOperator(Extent.Size*2);
            var map = opr.GetMappedFile(Extent.Size);
            opr.Dispose();
            map.Dispose();
            _file.Dispose();
            var f = System.IO.File.Open(FileName, FileMode.OpenOrCreate);
            Assert.AreEqual(Extent.Size * 2, f.Length);
            f.Dispose();
        }
    }
}
