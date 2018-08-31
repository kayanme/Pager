using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Implementations;
using System.Text;

namespace Test.Paging.PhysicalLevel
{
    [TestClass]
    public class BufferedPageFactoryTests
    {
        public TestContext TestContext { get; set; }
        private IGamAccessor GamMock => TestContext.Properties["IGAMAccessor"] as IGamAccessor;
        private IUnderlyingFileOperator FileMock => TestContext.Properties["IUnderlyingFileOperator"] as IUnderlyingFileOperator;
        private IExtentAccessorFactory BlockMock => TestContext.Properties["IExtentAccessorFactory"] as IExtentAccessorFactory;
        private IHeaderFactory HeaderMock => TestContext.Properties["IHeaderFactory"] as IHeaderFactory;
        private IBufferedPageFactory bufferedPageFactory()
        {
            return new BufferedPageFactory(HeaderMock,BlockMock,GamMock);
        }

        [TestMethod]
        public void CreatePage()
        {
        
        }


        [TestMethod]
        public void CreateHeaderedPage()
        {

        }
    }
}
