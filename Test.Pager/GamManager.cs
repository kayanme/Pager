using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pager;
using Rhino.Mocks;
using Test.Pager;

namespace Test.Pager
{
    [TestClass]
    public class GamManagerTest
    {
        public TestContext TestContext { get; set; }


        private static byte[] CreateEmptyGAM() => new byte[Extent.Size];

    
        private static void MarkInGamFilled(byte[] gam, int pageNum,byte type)
        {          
            gam[pageNum] = type;
        }

        private static void MarkInGamEmpty(byte[] gam, int pageNum)
        {
          
            gam[pageNum] = 0;
        }
     
        private void CheckFile(byte[] gam)
        {
            _map.Dispose();
            var file = File.ReadAllBytes(FileName);
            CollectionAssert.AreEqual(gam, file);
        }
        private string FileName=>TestContext.TestName;
        private MemoryMappedFile _map;
        private IGAMAccessor GetManager()
        {
            _map = MemoryMappedFile.CreateFromFile(FileName, System.IO.FileMode.OpenOrCreate, FileName, Extent.Size);
            var file = new MockRepository().StrictMock<IUnderlyingFileOperator>();
            file.Expect(k => k.GetMappedFile(Extent.Size)).Return(_map);
            file.Replay();
            return new GAMAccessor(file);
        }
        [TestCleanup]
        public void Clean()
        {
            try
            {
              
                File.Delete(FileName);
            }
            catch { }
        }
        [TestMethod]
        public void MarkUsed()
        {

            using (var manager = GetManager())
            {
                var pageNum = manager.MarkPageUsed(1);
                Assert.AreEqual(0, pageNum);
            }

            var gam = CreateEmptyGAM();
            MarkInGamFilled(gam, 0, 1);
            CheckFile(gam);
        }

        [TestMethod]
        public void MarkUsed_WhenOnePageUsed()
        {
            var gam = CreateEmptyGAM();
            MarkInGamFilled(gam, 0, 1);
            using (var file = File.Create(FileName))
            {
                file.Write(new byte[] { 1 }, 0, 1);
            }

            using (var manager = GetManager())
            {
                var pageNum = manager.MarkPageUsed(2);
                Assert.AreEqual(1, pageNum);
            }
          
            MarkInGamFilled(gam, 1, 2);
            CheckFile(gam);
        }


        [TestMethod]
        public void MarkFree()
        {
            var gam = CreateEmptyGAM();
            MarkInGamFilled(gam, 0, 1);
            using (var file = File.Create(FileName))
            {
                file.Write(new byte[] { 1 }, 0, 1);
            }

            using (var manager = GetManager())
            {
                manager.MarkPageFree(0);
              
            }

            MarkInGamEmpty(gam, 0);
            CheckFile(gam);
        }
    }
}
