using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using File.Paging.PhysicalLevel;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
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
            config.PageMap.Add(3, vconfig);
            config.HeaderConfig.Add(3, hconfig); 

            var manager = new PageManager(config, gamMock, blockFactoryMock,fileOperator);
            TestContext.Properties.Add("manager", manager);
            TestContext.Properties.Add("fconfig", fconfig);
            TestContext.Properties.Add("vconfig", vconfig);
        }

        private IPageManager GetManager()
        {
                    
            return TestContext.Properties["manager"] as IPageManager; ;
        }

        private string FileName => TestContext.TestName;
        private byte Fconfig => 1;
        private byte Vconfig => 2;


        private IGamAccessor GamMock => TestContext.Properties["IGAMAccessor"] as IGamAccessor;
        private IUnderlyingFileOperator FileMock => TestContext.Properties["IUnderlyingFileOperator"] as IUnderlyingFileOperator;
        private IExtentAccessorFactory BlockMock => TestContext.Properties["IExtentAccessorFactory"] as IExtentAccessorFactory;

     
        [TestMethod]
        public void FixedPageCreation()
        {
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
         
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.MarkPageUsed(1)).Return(0);
            GamMock.Expect(k => k.GetPageType(0)).Return(1);
            var manager = GetManager();
            var page = manager.CreatePage(Fconfig);
            manager.Dispose();
           
          
            GamMock.VerifyAllExpectations();
          
        }

        [TestMethod]
        public void VariablePageCreation()
        {
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
       
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.MarkPageUsed(2)).Return(0);
            GamMock.Expect(k => k.GetPageType(0)).Return(2);
            var manager = GetManager();
            var page = manager.CreatePage(Vconfig);
            manager.Dispose();

            t.VerifyAllExpectations();
            BlockMock.VerifyAllExpectations();
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
        public void FixedPageAcqure()
        {

            var t = MockRepository.GenerateStrictMock<IPageAccessor>();           
         
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(0)).Return(1);
            using (var manager = GetManager())
            {
                var page = manager.RetrievePage(new PageReference(0));              
                Assert.AreEqual(new PageReference(0), page.Reference);
                Assert.IsInstanceOfType(page, typeof(FixedRecordTypedPage<TestRecord>));
            }
            BlockMock.VerifyAllExpectations();
        }

        [TestMethod]
        public void VariablePageAcqure()
        {

            var t = MockRepository.GenerateStrictMock<IPageAccessor>();

            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(0)).Return(2);
            using (var manager = GetManager())
            {
                var page = manager.RetrievePage(new PageReference(0));
                Assert.AreEqual(new PageReference(0), page.Reference);
                Assert.IsInstanceOfType(page, typeof(ComplexRecordTypePage<TestRecord>));
            }
            BlockMock.VerifyAllExpectations();
        }

       


        [TestMethod]
        public void HeaderedPageAcquire()
        {

            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.GetChildAccessorWithStartShift(7)).Repeat.Once().Return(null);
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            t.Expect(k => k.GetChildAccessorWithStartShift(0)).Return(t);
            BlockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            GamMock.Expect(k => k.GetPageType(0)).Return(3);
            using (var manager = GetManager())
            {
                var page = manager.RetrievePage(new PageReference(0));
                Assert.AreEqual(new PageReference(0), page.Reference);
                Assert.IsInstanceOfType(page, typeof(HeaderedPage<TestHeader>));            
            }
          
            BlockMock.VerifyAllExpectations();
        }

    }
}
