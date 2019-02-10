using System.IO;
using System.IO.Paging.PhysicalLevel;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Implementations;
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
        private static int _extentSize = 64 * 1024;
        private IUnderlyingFileOperator GetOperator(int initSize)
        {
            _file = System.IO.File.Open(FileName, FileMode.OpenOrCreate);
            _file.SetLength(initSize);
            _file.Flush();
            return new UnderyingFileOperator(_file,
                new PageManagerConfiguration { ExtentSize = _extentSize });
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
            var opr = GetOperator(_extentSize);
            var map = opr.GetMappedFile(_extentSize * 2);
            opr.Dispose();
            map.Dispose();            
            _file.Dispose();
            var f = System.IO.File.Open(FileName, FileMode.OpenOrCreate);
            Assert.AreEqual(_extentSize * 2, f.Length);
            f.Dispose();
        }

        [TestMethod]
        public void TakeMap()
        {
            var opr = GetOperator(_extentSize * 2);
            var map = opr.GetMappedFile(_extentSize);
            opr.Dispose();
            map.Dispose();
            _file.Dispose();
            var f = System.IO.File.Open(FileName, FileMode.OpenOrCreate);
            Assert.AreEqual(_extentSize * 2, f.Length);
            f.Dispose();
        }
    }
}
