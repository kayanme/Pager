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
            config.PageMap.Add(1, new FixedRecordTypePageConfiguration<TestRecord>
            {
                RecordType = new RecordDeclaration<TestRecord>((t, b) => { t.FillFromByteArray(b); }, (b, t) => { t.FillByteArray(b); }, 7)
            });

            var manager = new PageManager(config, gamMock, blockFactoryMock,fileOperator);
            TestContext.Properties.Add("manager", manager);
        }

        private IPageManager GetManager()
        {
                    
            return TestContext.Properties["manager"] as IPageManager; ;
        }

        private string FileName => TestContext.TestName;


        private IGAMAccessor gamMock => TestContext.Properties["IGAMAccessor"] as IGAMAccessor;
        private IUnderlyingFileOperator fileMock => TestContext.Properties["IUnderlyingFileOperator"] as IUnderlyingFileOperator;
        private IExtentAccessorFactory blockMock => TestContext.Properties["IExtentAccessorFactory"] as IExtentAccessorFactory;

     
        [TestMethod]
        public void PageCreation()
        {
            var t = MockRepository.GenerateStrictMock<IPageAccessor>();
            t.Expect(k => k.PageSize).Repeat.Any().Return(4096);
            t.Expect(k => k.GetByteArray(0, 4096)).Return(new byte[4096]);
            t.Expect(k => k.Dispose()).Repeat.Any();
            blockMock.Expect(k => k.GetAccessor(Extent.Size, 4096)).Return(t);
            gamMock.Expect(k => k.MarkPageUsed(1)).Return(0);         
            var manager = GetManager();
            var page = manager.CreatePage<TestRecord>();
            manager.Dispose();
           
          
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

          
            blockMock.Expect(k => k.GetAccessor(Extent.Size, 4096));
            using (var manager = GetManager())
            {
                var page = manager.RetrievePage<TestRecord>(new PageReference(0));              
                Assert.AreEqual(new PageReference(0), page.Reference);
            }
            blockMock.VerifyAllExpectations();
        }

    }
}
