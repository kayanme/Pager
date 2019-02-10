using System;
using System.Linq;
using FakeItEasy;
using System.IO.Paging.PhysicalLevel;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;

//using Rhino.Mocks;

namespace Test.Paging.PhysicalLevel
{
    [TestClass]
    public class PageManagerTest
    {

        public TestContext TestContext { get; set; }
        private static int _extentSize = 64 * 1024;
        [TestInitialize]
        public void Init()
        {
          
            var gamMock = A.Fake<IGamAccessor>();
            
            var blockFactoryMock = A.Fake<IExtentAccessorFactory>();
            var fileOperator = A.Fake<IUnderlyingFileOperator>();
            TestContext.Properties.Add("IGAMAccessor", gamMock);
            TestContext.Properties.Add("IExtentAccessorFactory", blockFactoryMock);
            TestContext.Properties.Add("IUnderlyingFileOperator", fileOperator);
            var config = new PageManagerConfiguration()
            {
                SizeOfPage = PageManagerConfiguration.PageSize.Kb4,
                ExtentSize = _extentSize
            };
            var fconfig = new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordMap = new FixedSizeRecordDeclaration<TestRecord>((ref TestRecord t, byte[] b) => { t.FillFromByteArray(b); }, (byte[] b, ref TestRecord t) => { t.FillByteArray(b); }, 7)
            };

            var vconfig = new VariableRecordTypePageConfiguration<TestRecord>
            {
              
                
                //RecordType = new RecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            };
            var hconfig = new PageHeadersConfiguration<TestHeader>
            {
                Header = new FixedSizeRecordDeclaration<TestHeader>((ref TestHeader t, byte[] b) => { t.FillFromByteArray(b); }, (byte[] b, ref TestHeader t) => { t.FillByteArray(b); },7),
                InnerPageMap = fconfig
            };
           

            config.PageMap.Add(1, fconfig);
            config.PageMap.Add(2, vconfig);
            config.PageMap.Add(3, fconfig);
            config.HeaderConfig.Add(3, hconfig);
      
            var pageFact = A.Fake<IPageFactory>();
            var pageBuffer = A.Fake<IPageBuffer>();
            
            var manager = new PageManager(config, gamMock,fileOperator, pageFact, pageBuffer);
            TestContext.Properties.Add("manager", manager);
            TestContext.Properties.Add("fconfig", fconfig);
            TestContext.Properties.Add("hconfig", hconfig);
            TestContext.Properties.Add("pageBuffer", pageBuffer);
            TestContext.Properties.Add("pageFact", pageFact);
            A.CallTo(() => GamMock.GamShift(A<int>.Ignored)).Returns(config.ExtentSize);
        }

        private IPageManager GetManager()
        {
                    
            return TestContext.Properties["manager"] as IPageManager; ;
        }

        private string FileName => TestContext.TestName;
        private byte Fconfig => 1;
        private byte Vconfig => 2;


        private IPageBuffer pageBuffer => TestContext.Properties["pageBuffer"] as IPageBuffer;
        private IPageFactory pageFactory => TestContext.Properties["pageFact"] as IPageFactory;
        private IGamAccessor GamMock => TestContext.Properties["IGAMAccessor"] as IGamAccessor;
        private IUnderlyingFileOperator FileMock => TestContext.Properties["IUnderlyingFileOperator"] as IUnderlyingFileOperator;
        private IExtentAccessorFactory BlockMock => TestContext.Properties["IExtentAccessorFactory"] as IExtentAccessorFactory;

     
        [TestMethod]
        public void FixedPageCreation()
        {
            var t = A.Fake<IPageAccessor>();
            A.CallTo(() => t.PageSize).Returns(4096);

            A.CallTo(() => t.GetByteArray(0, 4096)).Returns(new byte[4096]);
            A.CallTo(() => t.GetByteArray(0, 72)).Returns(new byte[72]);         
            A.CallTo(() => t.GetChildAccessorWithStartShift(0)).Returns(t);
            A.CallTo(() => BlockMock.GetAccessor(0, 4096, _extentSize)).Returns(t);
            A.CallTo(()=> GamMock.MarkPageUsed(1)).Returns(0);
            A.CallTo(() => GamMock.GamShift(1)).Returns(_extentSize);

            var manager = GetManager();
            var page = manager.CreatePage(Fconfig);
            manager.Dispose();
        
        }

     

        [TestMethod]
        public void PageDeletion()
        {                            
          
            var manager = GetManager();
            manager.DeletePage(new PageReference(0));
            manager.Dispose();

            A.CallTo(() => GamMock.MarkPageFree(0)).MustHaveHappened();

        }


        [TestMethod]
        public void PageAcqure()
        {
          
            var bufpage = A.Dummy<BufferedPage>();               
            A.CallTo(() => GamMock.GamShift(0)).Returns(_extentSize);
            A.CallTo(() => pageBuffer.GetPageFromBuffer(A<PageReference>.That.Matches(pr => pr.PageNum == 0),
                A<PageManagerConfiguration>.Ignored,4096, _extentSize))
                .Returns(bufpage);
            var p = A.Fake<IPageInfo>();
            
            A.CallTo(() => pageFactory.GetPageInfo(A<BufferedPage>.That.IsSameAs(bufpage),
                                                  A<PageReference>.That.Matches(pr=> pr.PageNum == 0),
                                                  A<Action>.That.IsNotNull()))
                .Returns(p);
          
            A.CallTo(() => GamMock.GetPageType(0)).Returns<byte>(1);

            using (var manager = GetManager())
            {
                var page = manager.GetPageInfo(new PageReference(0));              
                Assert.AreEqual(p, page);
              
            }
        }                    
        
   
        [TestMethod]
        public void PageIteration()
        {
            var t = A.Fake<IPageAccessor>();
            A.CallTo(()=> GamMock.GetPageType(0)).Returns<byte>(1);
            A.CallTo(() => GamMock.GetPageType(A<int>.That.Not.IsEqualTo(0))).Returns<byte>(0);
            using (var manager = GetManager())
            {
                var pages = manager.IteratePages(1).ToArray();
                Assert.AreEqual(1,pages.Length);
                Assert.AreEqual(new PageReference(0), pages[0]);            
            }           
        }


        [TestMethod]
        public void RemovePageFromBuffer()
        {
         
            var bufpage = A.Dummy<BufferedPage>();         
            var p = A.Fake<IPageInfo>();
            A.CallTo(() => pageBuffer.GetPageFromBuffer(
                A<PageReference>.That.Matches(pr => pr.PageNum == 1),
                A<PageManagerConfiguration>.Ignored, 4096,_extentSize))
              .Returns(bufpage);

            A.CallTo(() => pageFactory.GetPageInfo(A<BufferedPage>.That.IsSameAs(bufpage), new PageReference(1), A<Action>.That.IsNotNull()))
                .ReturnsLazily(
                    (a) => {
                        A.CallTo(() => p.Dispose()).Invokes(_ => (a.Arguments[2] as Action)()); return p;
                    });
       
            using (var manager = GetManager())
            {           
                using (var page = manager.GetPageInfo(new PageReference(1)))
                {
                    (manager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(new PageReference(1));
                    A.CallTo(() => pageBuffer.ReleasePageUseAndCleanIfNeeded(new PageReference(1), bufpage)).MustNotHaveHappened();
                }
                A.CallTo(() => pageBuffer.ReleasePageUseAndCleanIfNeeded(new PageReference(1), bufpage)).MustHaveHappened();
            }         
        }
    }
}

