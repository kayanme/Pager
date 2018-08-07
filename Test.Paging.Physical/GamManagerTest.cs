using System.IO;
using System.IO.MemoryMappedFiles;
using FakeItEasy;
using File.Paging.PhysicalLevel;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Test.Pager
{
    [TestClass]
    public class GamManagerTest
    {
        public TestContext TestContext { get; set; }


        private static byte[] CreateEmptyGam() => new byte[Extent.Size];

    
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
            var file = System.IO.File.ReadAllBytes(FileName);
            CollectionAssert.AreEqual(gam, file);
        }
        private string FileName=>TestContext.TestName;
        private MemoryMappedFile _map;
        private IGamAccessor GetManager()
        {
            _map = MemoryMappedFile.CreateFromFile(FileName, FileMode.OpenOrCreate, FileName, Extent.Size);
            var file = A.Fake<IUnderlyingFileOperator>(s=>s.Strict());
            A.CallTo(()=> file.GetMappedFile(Extent.Size)).Returns(_map);
            A.CallTo(() => file.ReturnMappedFile(_map)).DoesNothing();           
            return new GamAccessor(file);
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
        public void MarkUsed()
        {

            using (var manager = GetManager())
            {
                var pageNum = manager.MarkPageUsed(1);
                Assert.AreEqual(0, pageNum);
            }

            var gam = CreateEmptyGam();
            MarkInGamFilled(gam, 0, 1);
            CheckFile(gam);
        }

        [TestMethod]
        public void MarkUsed_WhenOnePageUsed()
        {
            var gam = CreateEmptyGam();
            MarkInGamFilled(gam, 0, 1);
            using (var file = System.IO.File.Create(FileName))
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
            var gam = CreateEmptyGam();
            MarkInGamFilled(gam, 0, 1);
            using (var file = System.IO.File.Create(FileName))
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
