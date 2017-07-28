using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pager;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using Rhino.Mocks;
using System.IO.MemoryMappedFiles;
using Pager.Classes;

namespace Test.Pager
{
    [TestClass]
    public class PageManagerTest
    {

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            var gamMock = MockRepository.GenerateMock<IGAMAccessor>();
            var blockFactoryMock = MockRepository.GenerateMock<IExtentAccessorFactory>();
            var fileOperator = MockRepository.GenerateMock<IUnderlyingFileOperator>();
            TestContext.Properties.Add("IGAMAccessor", gamMock);
            TestContext.Properties.Add("IExtentAccessorFactory", blockFactoryMock);
            TestContext.Properties.Add("IUnderlyingFileOperator", fileOperator);
            var config = new PageManagerConfiguration();
            config.SizeOfPage = PageManagerConfiguration.PageSize.Kb4;
            var fconfig = new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordType = new FixedSizeRecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            };

            var vconfig = new VariableRecordTypePageConfiguration<TestRecord>
            {
              
                
                //RecordType = new RecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            };

            config.PageMap.Add(1, fconfig);
            config.PageMap.Add(2, vconfig);

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
        private byte fconfig => 1;
        private byte vconfig => 2;


        private IGAMAccessor gamMock => TestContext.Properties["IGAMAccessor"] as IGAMAccessor;
        private IUnderlyingFileOperator fileMock => TestContext.Properties["IUnderlyingFileOperator"] as IUnderlyingFileOperator;
        private IExtentAccessorFactory blockMock => TestContext.Properties["IExtentAccessorFactory"] as IExtentAccessorFactory;

     
        [TestMethod]
        public void FixedPageCreation()
        {
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
         
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            blockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            gamMock.Expect(k => k.MarkPageUsed(1)).Return(0);
            gamMock.Expect(k => k.GetPageType(0)).Return(1);
            var manager = GetManager();
            var page = manager.CreatePage(fconfig);
            manager.Dispose();
           
          
            gamMock.VerifyAllExpectations();
          
        }

        [TestMethod]
        public void VariablePageCreation()
        {
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
       
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            blockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            gamMock.Expect(k => k.MarkPageUsed(2)).Return(0);
            gamMock.Expect(k => k.GetPageType(0)).Return(2);
            var manager = GetManager();
            var page = manager.CreatePage(vconfig);
            manager.Dispose();

            t.VerifyAllExpectations();
            blockMock.VerifyAllExpectations();
            gamMock.VerifyAllExpectations();

        }

        [TestMethod]
        public void PageDeletion()
        {                            

            gamMock.Expect(k => k.MarkPageFree(0));
            var manager = GetManager();
            manager.DeletePage(new PageReference(0),true);
            manager.Dispose();
                     
            gamMock.VerifyAllExpectations();
            
        }


        [TestMethod]
        public void PageAcqure()
        {

            var t = MockRepository.GenerateStrictMock<IPageAccessor>();           
         
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            blockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            gamMock.Expect(k => k.GetPageType(0)).Return(1);
            using (var manager = GetManager())
            {
                var page = manager.RetrievePage(new PageReference(0));              
                Assert.AreEqual(new PageReference(0), page.Reference);
            }
            blockMock.VerifyAllExpectations();
        }

    }
}
