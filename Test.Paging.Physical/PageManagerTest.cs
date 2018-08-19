using System;
using System.Linq;
using FakeItEasy;
using File.Paging.PhysicalLevel;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.PageFactories;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//using Rhino.Mocks;

namespace Test.Paging.PhysicalLevel
{
    [TestClass]
    public class PageManagerTest
    {

        public TestContext TestContext { get; set; }

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
                SizeOfPage = PageManagerConfiguration.PageSize.Kb4
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
            var headerFact = A.Fake<IHeaderFactory>();
            var manager = new PageManager(config, gamMock, blockFactoryMock,fileOperator, pageFact, headerFact);
            TestContext.Properties.Add("manager", manager);
            TestContext.Properties.Add("fconfig", fconfig);
            TestContext.Properties.Add("hconfig", hconfig);
            TestContext.Properties.Add("headerFact", headerFact);
            TestContext.Properties.Add("pageFact", pageFact);
            A.CallTo(() => GamMock.GamShift(A<int>.Ignored)).Returns(Extent.Size);
        }

        private IPageManager GetManager()
        {
                    
            return TestContext.Properties["manager"] as IPageManager; ;
        }

        private string FileName => TestContext.TestName;
        private byte Fconfig => 1;
        private byte Vconfig => 2;


        private IHeaderFactory headerFactory => TestContext.Properties["headerFact"] as IHeaderFactory;
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
            A.CallTo(() => BlockMock.GetAccessor(Extent.Size, 4096)).Returns(t);
            A.CallTo(()=> GamMock.MarkPageUsed(1)).Returns(0);
            A.CallTo(() => GamMock.GamShift(1)).Returns(Extent.Size);

            var manager = GetManager();
            var page = manager.CreatePage(Fconfig);
            manager.Dispose();
        
        }

     

        [TestMethod]
        public void PageDeletion()
        {                            

          
            var manager = GetManager();
            manager.DeletePage(new PageReference(0),true);
            manager.Dispose();

            A.CallTo(() => GamMock.MarkPageFree(0)).MustHaveHappened();

        }


        [TestMethod]
        public void PageAcqure()
        {

            var t = A.Fake<IPageAccessor>();
            var h = A.Fake<IPageHeaders>();
       
            A.CallTo(()=> t.PageSize).Returns(4096);
            A.CallTo(() => t.GetByteArray(0, 4096)).Returns(new byte[4096]);
            A.CallTo(() => t.GetByteArray(0, 72)).Returns(new byte[72]);//заголовок

            A.CallTo(() => GamMock.GamShift(0)).Returns(Extent.Size);
            A.CallTo(() => headerFactory.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t,null))
                .Returns(h);
            var p = A.Fake<IPageInfo>();
     
            A.CallTo(() => pageFactory.GetPageInfo(A<BufferedPage>.That.Matches(k=> !k.MarkedForRemoval
                                                                                    && k.UserCount == 1
                                                                                    && k.Accessor == t
                                                                                    && k.ContentAccessor == t
                                                                                    && k.HeaderConfig == null
                                                                                    && k.Headers == h
                                                                                    && k.PageType == 1, null, null),
                                                  A<PageReference>.That.Matches(pr=> pr.PageNum == 0),
                                                  A<Action>.That.IsNotNull()))
                .Returns(p);
            A.CallTo(()=> BlockMock.GetAccessor(Extent.Size, 4096)).Returns(t);
            A.CallTo(() => GamMock.GetPageType(0)).Returns<byte>(1);

            using (var manager = GetManager())
            {
                var page = manager.GetPageInfo(new PageReference(0));              
                Assert.AreEqual(p, page);
              
            }



        }

       

       


        [TestMethod]
        public void HeaderedPageAcquire()
        {

            var t = A.Fake<IPageAccessor>();
            A.CallTo(() => t.GetChildAccessorWithStartShift(7)).Returns(t);
            A.CallTo(() => t.PageSize).Returns(4096);
            A.CallTo(() => t.GetByteArray(0, 4096)).Returns(new byte[4096]);
            A.CallTo(() => t.GetByteArray(0, 72)).Returns(new byte[72]);//получаем заголовок
            
            A.CallTo(()=> BlockMock.GetAccessor(Extent.Size, 4096)).Returns(t);
            A.CallTo(() => GamMock.GetPageType(0)).Returns<byte>(3);
            A.CallTo(() => GamMock.GamShift(0)).Returns(Extent.Size);
            var h = A.Fake<IPageHeaders>();
            var p = A.Fake<IPageInfo>();
            A.CallTo(() => headerFactory.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t, TestContext.Properties["hconfig"] as PageHeadersConfiguration))
                .Returns(h);
            A.CallTo(() => pageFactory.GetPageInfo(A<BufferedPage>.That.Matches(
                    k => !k.MarkedForRemoval
                         && k.UserCount == 1
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == TestContext.Properties["hconfig"] as PageHeadersConfiguration
                         && k.Headers == h
                         && k.PageType == 3),new PageReference(0),A<Action>.That.IsNotNull()))
                .Returns(p as IPageInfo);


            using (var manager = GetManager())
            {
                var page = manager.GetPageInfo(new PageReference(0));
                Assert.AreEqual(p, page);                      
            }
            A.CallTo(() => t.Dispose()).MustHaveHappened();
        }
        
        [TestMethod]
        public void RemovePageFromBuffer()
        {
            var t = A.Fake<IPageAccessor>();

            A.CallTo(() => t.PageSize).Returns(4096);
            A.CallTo(() => t.GetByteArray(0, 4096)).Returns(new byte[4096]);
            A.CallTo(() => t.GetByteArray(0, 72)).Returns(new byte[72]);//заголовок
          
            A.CallTo(() => t.GetChildAccessorWithStartShift(0)).Returns(t);
            
            A.CallTo(() => BlockMock.GetAccessor(Extent.Size+4096, 4096)).Returns(t).Once();
            A.CallTo(() => GamMock.GetPageType(1)).Returns<byte>(1);
            A.CallTo(() => GamMock.GamShift(1)).Returns(Extent.Size);
            var h = A.Fake<IPageHeaders>();
            var p = A.Fake<IPageInfo>();
            A.CallTo(() => headerFactory.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t, null))
                .Returns(h);
            A.CallTo(() => pageFactory.GetPageInfo(
                A<BufferedPage>.That.Matches(
                    k => !k.MarkedForRemoval
                         && k.UserCount == 1
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == null
                         && k.Headers == h
                         && k.PageType == 1),new PageReference(1), A<Action>.That.IsNotNull()))                  
                .ReturnsLazily(
                    (a)=> { A.CallTo(() => p.Dispose()).Invokes(_=>(a.Arguments[2] as Action)()); return p;
                });
            bool pageRemoved = false;

            using (var manager = GetManager())
            {
                (manager as IPhysicalPageManipulation).PageRemovedFromBuffer += (_, ea) => {Assert.AreEqual(1,ea.Page.PageNum); pageRemoved = true; };
                using (var page = manager.GetPageInfo(new PageReference(1)))
                {
                    (manager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(new PageReference(1));
                    Assert.IsFalse(pageRemoved);
                }
                Assert.IsTrue(pageRemoved);
            }
            A.CallTo(() => t.Dispose()).MustHaveHappened();
        }


        [TestMethod]
        public void RemovePageFromBufferWithConcurrentCreation()
        {
            var t = A.Fake<IPageAccessor>();

            A.CallTo(() => t.PageSize).Returns(4096);
            A.CallTo(() => t.GetByteArray(0, 4096)).Returns(new byte[4096]);
            A.CallTo(() => t.GetByteArray(0, 72)).Returns(new byte[72]);//заголовок
            
            A.CallTo(() => t.GetChildAccessorWithStartShift(0)).Returns(t);

            A.CallTo(() => BlockMock.GetAccessor(Extent.Size + 4096, 4096)).Returns(t);
            A.CallTo(() => GamMock.GetPageType(1)).Returns<byte>(1);
            var h = A.Fake<IPageHeaders>();
            var p = A.Fake<IPageInfo>();
            var p2 = A.Fake<IPageInfo>();
            A.CallTo(() => headerFactory.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t, null))
                .Returns(h);

            A.CallTo(() => pageFactory.GetPageInfo(A<BufferedPage>.That.Matches(
                    k => !k.MarkedForRemoval
                         && k.UserCount == 1
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == null
                         && k.Headers == h
                         && k.PageType == 1),new PageReference(1), A<Action>.That.IsNotNull()))
                .ReturnsLazily(a=> { A.CallTo(() => p.Dispose()).Invokes(a.Arguments[2] as Action); return p; });
            A.CallTo(() => pageFactory.GetPageInfo(A<BufferedPage>.That.Matches(
                    k => k.MarkedForRemoval
                         && k.UserCount == 2
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == null
                         && k.Headers == h
                         && k.PageType == 1), new PageReference(1), A<Action>.That.IsNotNull()))
                 .ReturnsLazily(a => { A.CallTo(() => p.Dispose()).Invokes(a.Arguments[2] as Action); return p; });

            
            bool pageRemoved = false;
            using (var manager = GetManager())
            {
                IPageInfo page2;
                (manager as IPhysicalPageManipulation).PageRemovedFromBuffer += (_, ea) => { Assert.AreEqual(1, ea.Page.PageNum); pageRemoved = true; };
                using (var page = manager.GetPageInfo(new PageReference(1)))
                {
                    (manager as IPhysicalPageManipulation).MarkPageToRemoveFromBuffer(new PageReference(1));
                    Assert.IsFalse(pageRemoved);
                    page2 = manager.GetPageInfo(new PageReference(1));
                }
                Assert.IsFalse(pageRemoved);
                page2.Dispose();
                Assert.IsTrue(pageRemoved);
            }
            A.CallTo(() => t.Dispose()).MustHaveHappened();           
           
        }



        [TestMethod]
        public void PageIteration()
        {

            var t = A.Fake<IPageAccessor>();

//A.CallTo(()=>            //t.PageSize).Returns(4096);
//A.CallTo(()=>            //t.GetByteArray(0, 4096)).Returns(new byte[4096]);
//A.CallTo(()=>            //t.GetByteArray(0, 72)).Returns(new byte[72]);//заголовок
//A.CallTo(()=>            //t.Dispose());
//A.CallTo(()=>            //t.GetChildAccessorWithStartShift(0)).Returns(t);
//A.CallTo(()=>            //BlockMock.GetAccessor(Extent.Size, 4096)).Returns(t);
            A.CallTo(()=> GamMock.GetPageType(0)).Returns<byte>(1);
            A.CallTo(() => GamMock.GetPageType(A<int>.That.Not.IsEqualTo<int>(0))).Returns<byte>(0);
            using (var manager = GetManager())
            {
                var pages = manager.IteratePages(1).ToArray();
                Assert.AreEqual(1,pages.Length);
                Assert.AreEqual(new PageReference(0), pages[0]);            
            }
           
        }
    }
}

