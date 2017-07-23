//using System;
//using System.ComponentModel.Composition.Hosting;
//using System.IO;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Pager;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Collections.Generic;
//using Rhino.Mocks;
//using System.IO.MemoryMappedFiles;

//namespace Test.Pager
//{
   
//    public class PageManagerTes2
//    {

//        public TestContext TestContext { get; set; }

//        private IPageManager GetManager()
//        {
//            var gamMock = MockRepository.GenerateMock<IGAMAccessor>();
//            var config = new PageMapConfiguration();
//            config.PageMap.Add(1, typeof(TestRecord));
//          //  var manager = new PageManager(config, gamMock, File.Open(FileName,FileMode.Open));
//            return null;
//        }

//        private string FileName => TestContext.TestName;

//        private const short GAMLength = 8192;

//        private static byte[] CreateEmptyGAM() => new byte[GAMLength];

//        private static byte[] CreateEmptyPage() => new byte[TypedPage.PageSize];

//        private static void MarkInGamFilled(byte[] gam,int pageNum)
//        {
//            var m = 1<< ((byte)pageNum % 8);

//            gam[pageNum / 8] = (byte)(gam[pageNum / 8] | (byte)m);
//        }

//        private static void MarkInGamEmpty(byte[] gam, int pageNum)
//        {
//            var m = 1 << ((byte)pageNum % 8);
//            m = ~m;
//            gam[pageNum / 8] = (byte)(gam[pageNum / 8] & (byte)m);
//        }

//        private static byte[] ConcatPages(params byte[][] pages) => 
//            pages.OfType<IEnumerable<byte>>().Aggregate((s, a) => s.Concat(a)).ToArray();

//        private void CheckFile(params byte[][] pages)
//        {
//            CheckFile(pages as IEnumerable<byte[]>);
//        }

//        private void CheckFile(IEnumerable<byte[]> pages)
//        {
//            var file = File.ReadAllBytes(FileName);
//            Assert.AreEqual(ConcatPages(pages.ToArray()), file);
//        }



//        [TestCleanup]
//        public void Clean()
//        {
//            try
//            {
//                File.Delete(FileName);
//            }
//            catch
//            {

//            }
//        }

//        [TestMethod]
//        public void ManagerCreation()
//        {
//            var manager = GetManager();
//            manager.Dispose();
//            CheckFile(CreateEmptyGAM());
//        }

//        [TestMethod]
//        public void ManagerCreation_FileExists()
//        {
//            File.Create(FileName);
//            var manager = GetManager();
//            manager.Dispose();
//            CheckFile(CreateEmptyGAM());
//        }

//        [TestMethod]
//        public void PageCreation()
//        {
//            var manager = GetManager();
//            var page = manager.CreatePage<TestRecord>();
//            manager.Dispose();

//            var gam = CreateEmptyGAM();
//            MarkInGamFilled(gam, 0);
//            var pages = new[] { gam }.Concat(Enumerable.Range(0,TypedPage.PageInExtentCount).Select(k=>CreateEmptyPage())).ToArray();
//            CheckFile(pages);
//        }


//        [TestMethod]
//        public void PageDeletion()
//        {
//            var gam = CreateEmptyGAM();
//            MarkInGamFilled(gam, 0);
//            var spages = Enumerable.Range(0, TypedPage.PageInExtentCount).Select(k => CreateEmptyPage());
//            var pages = new[] { gam }.Concat(spages).ToArray();
//            File.WriteAllBytes(FileName, ConcatPages(pages));

//            var manager = GetManager();
//            manager.DeletePage(new PageReference(1),true);
//            manager.Dispose();

//            MarkInGamEmpty(gam, 0);
//            pages = new[] { gam }.Concat(spages).ToArray();
//            CheckFile(pages);
//        }


//        [TestMethod]
//        public void PageAcqure()
//        {
//            var gam = CreateEmptyGAM();
//            MarkInGamFilled(gam, 0);
//            var spages = Enumerable.Range(0, TypedPage.PageInExtentCount).Select(k => CreateEmptyPage());
//            var pages = new[] { gam }.Concat(spages).ToArray();
//            File.WriteAllBytes(FileName, ConcatPages(pages));

//            using (var manager = GetManager())
//            {
//                var page = manager.RetrievePage<TestRecord>(new PageReference(1));
//                Assert.AreEqual(0.0, page.PageFullness);
//                Assert.AreEqual(new PageReference(1), page.Reference);
//            }

//            MarkInGamEmpty(gam, 0);
//            pages = new[] { gam }.Concat(spages).ToArray();
//            CheckFile(pages);
//        }

//    }
//}
