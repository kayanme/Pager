using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using File.Paging.PhysicalLevel;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.PageFactories;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace Test.Pager
{
    [TestClass]
    public class PageManagerTest
    {

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            var gamMock = MockRepository.GenerateMock<IGamAccessor>();
            var blockFactoryMock = MockRepository.GenerateMock<IExtentAccessorFactory>();
            var fileOperator = MockRepository.GenerateMock<IUnderlyingFileOperator>();
            TestContext.Properties.Add("IGAMAccessor", gamMock);
            TestContext.Properties.Add("IExtentAccessorFactory", blockFactoryMock);
            TestContext.Properties.Add("IUnderlyingFileOperator", fileOperator);
            var config = new PageManagerConfiguration()
            {
                SizeOfPage = PageManagerConfiguration.PageSize.Kb4
            };
            var fconfig = new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordMap = new FixedSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            };

            var vconfig = new VariableRecordTypePageConfiguration<TestRecord>
            {
              
                
                //RecordType = new RecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            };
            var hconfig = new PageHeadersConfiguration<TestHeader>
            {
                Header = new FixedSizeRecordDeclaration<TestHeader>((t, b) => { t.FillFromByteArray(b); },  (b, t) => { t.FillByteArray(b); },7),
                InnerPageMap = fconfig
            };
           

            config.PageMap.Add(1, fconfig);
            config.PageMap.Add(2, vconfig);
            config.PageMap.Add(3, fconfig);
            config.HeaderConfig.Add(3, hconfig);
      
            var pageFact = MockRepository.GenerateStub<IPageFactory>();
            var headerFact = MockRepository.GenerateStub<IHeaderFactory>();
            var manager = new PageManager(config, gamMock, blockFactoryMock,fileOperator,
                  pageFact, headerFact);
            TestContext.Properties.Add("manager", manager);
            TestContext.Properties.Add("fconfig", fconfig);
            TestContext.Properties.Add("hconfig", hconfig);
            TestContext.Properties.Add("headerFact", headerFact);
            TestContext.Properties.Add("pageFact", pageFact);
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
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
         
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.GetByteArray(0, 72)).Return(new byte[72]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.MarkPageUsed(1)).Return(0);
          
            var manager = GetManager();
            var page = manager.CreatePage(Fconfig);
            manager.Dispose();
           
          
            GamMock.VerifyAllExpectations();
          
        }

     

        [TestMethod]
        public void PageDeletion()
        {                            

            GamMock.Expect(k => k.MarkPageFree(0));
            var manager = GetManager();
            manager.DeletePage(new PageReference(0),true);
            manager.Dispose();
                     
            GamMock.VerifyAllExpectations();
            
        }


        [TestMethod]
        public void PageAcqure()
        {

            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            var h = MockRepository.GenerateStub<IPageHeaders>();
       
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.GetByteArray(0, 72)).Return(new byte[72]);//заголовок
            t.Expect(k => k.Dispose()).Repeat.Any();
        //    t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            headerFactory
                .Expect(k => k.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t,null))
                .Return(h);
            var p = MockRepository.GenerateStub<IPage>();
            pageFactory.Expect(k2 => k2.GetPageInfo(Arg<BufferedPage>.Matches(
                k=>!k.MarkedForRemoval
                 && k.UserCount == 1
                 && k.Accessor == t
                 && k.ContentAccessor == t
                 && k.HeaderConfig == null
                 && k.Headers == h
                 && k.PageType ==1),Arg.Is(new PageReference(0)),Arg<Action>.Is.NotNull))
                .Return(p);
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(0)).Return(1);
            using (var manager = GetManager())
            {
                var page = manager.GetPageInfo(new PageReference(0));              
                Assert.AreEqual(p, page);
              
            }
            pageFactory.VerifyAllExpectations();
            headerFactory.VerifyAllExpectations();
            BlockMock.VerifyAllExpectations();
        }

       

       


        [TestMethod]
        public void HeaderedPageAcquire()
        {

            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.GetChildAccessorWithStartShift(7)).Repeat.Once().Return(t);
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.GetByteArray(0, 72)).Return(new byte[72]);//получаем заголовок
            t.Expect(k => k.Dispose()).Repeat.Any();         
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(0)).Return(3);
            var h = MockRepository.GenerateStub<IPageHeaders>();
            var p = MockRepository.GenerateStub<IPage>();
            headerFactory
                .Expect(k => k.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t, TestContext.Properties["hconfig"] as PageHeadersConfiguration))
                .Return(h);
            pageFactory.Expect(k2 => k2.GetPageInfo(Arg<BufferedPage>.Matches(
                    k => !k.MarkedForRemoval
                         && k.UserCount == 1
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == TestContext.Properties["hconfig"] as PageHeadersConfiguration
                         && k.Headers == h
                         && k.PageType == 3),Arg.Is(new PageReference(0)),Arg<Action>.Is.NotNull))
                .Return(p as IPage);


            using (var manager = GetManager())
            {
                var page = manager.GetPageInfo(new PageReference(0));
                Assert.AreEqual(p, page);                      
            }
            pageFactory.VerifyAllExpectations();
            headerFactory.VerifyAllExpectations();
            BlockMock.VerifyAllExpectations();
        }
        
        [TestMethod]
        public void RemovePageFromBuffer()
        {
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();

            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.GetByteArray(0, 72)).Return(new byte[72]);//заголовок
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            t.Expect(k => k.Flush());
            BlockMock.Expect(k => k.GetAccessor(Extent.Size+4096, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(1)).Return(1);
            var h = MockRepository.GenerateStub<IPageHeaders>();
            var p = MockRepository.GenerateStub<IPage>();
            headerFactory
                .Expect(k => k.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t, null))
                .Return(h);
            pageFactory.Expect(k2 => k2.GetPageInfo(Arg<BufferedPage>.Matches(
                    k => !k.MarkedForRemoval
                         && k.UserCount == 1
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == null
                         && k.Headers == h
                         && k.PageType == 1),Arg.Is(new PageReference(1)), Arg<Action>.Is.NotNull))
                .Do(new Func<BufferedPage,PageReference,Action,IPage>(
                    (_,__,a)=> { p.Expect(k => k.Dispose()).Do(a);
                    return p;
                }));
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
            BlockMock.VerifyAllExpectations();
        }


        [TestMethod]
        public void RemovePageFromBufferWithConcurrentCreation()
        {
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();

            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.GetByteArray(0, 72)).Return(new byte[72]);//заголовок
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            t.Expect(k => k.Flush()).Repeat.Twice();
            BlockMock.Expect(k => k.GetAccessor(Extent.Size + 4096, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(1)).Return(1);
            var h = MockRepository.GenerateStub<IPageHeaders>();
            var p = MockRepository.GenerateStub<IPage>();
            var p2 = MockRepository.GenerateStub<IPage>();
            headerFactory
                .Expect(k => k.CreateHeaders(TestContext.Properties["fconfig"] as PageContentConfiguration, t, null))
                .Return(h);
            pageFactory.BackToRecord();
            pageFactory.Expect(k2 => k2.GetPageInfo(Arg<BufferedPage>.Matches(
                    k => !k.MarkedForRemoval
                         && k.UserCount == 1
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == null
                         && k.Headers == h
                         && k.PageType == 1),Arg.Is(new PageReference(1)), Arg<Action>.Is.NotNull))
                .Do(new Func<BufferedPage, PageReference, Action, IPage>(
                    (_, __, a) => {
                        p.Expect(k => k.Dispose()).Do(a);
                        return p;
                    }));
            pageFactory.Expect(k2 => k2.GetPageInfo(Arg<BufferedPage>.Matches(
                    k => k.MarkedForRemoval
                         && k.UserCount == 2
                         && k.Accessor == t
                         && k.ContentAccessor == t
                         && k.HeaderConfig == null
                         && k.Headers == h
                         && k.PageType == 1), Arg.Is(new PageReference(1)), Arg<Action>.Is.NotNull))
                .Do(new Func<BufferedPage, PageReference, Action, IPage>(
                    (_, __, a) => {
                        p2.Expect(k => k.Dispose()).Do(a);
                        return p2;
                    }));
            pageFactory.Replay();
            bool pageRemoved = false;
            using (var manager = GetManager())
            {
                IPage page2;
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
            BlockMock.VerifyAllExpectations();
        }



        [TestMethod]
        public void PageIteration()
        {

            var t = MockRepository.GenerateStrictMock<IPageAccessor>();

            //t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            //t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            //t.Expect(k => k.GetByteArray(0, 72)).Return(new byte[72]);//заголовок
            //t.Expect(k => k.Dispose()).Repeat.Any();
            //t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            //BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(0)).Return(1);
            GamMock.Expect(k => k.GetPageType(Arg<byte>.Is.NotEqual(0))).Return(0);
            using (var manager = GetManager())
            {
                var pages = manager.IteratePages(1).ToArray();
                Assert.AreEqual(1,pages.Length);
                Assert.AreEqual(new PageReference(0), pages[0]);            
            }
            BlockMock.VerifyAllExpectations();
        }
    }
}
