using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Implementations;
using System.Text;

namespace Test.Paging.PhysicalLevel.Pages
{
    [TestClass]
    public class PageBufferTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            var gamMock = A.Fake<IGamAccessor>();

            var blockFactoryMock = A.Fake<IExtentAccessorFactory>();          
            TestContext.Properties.Add("IGAMAccessor", gamMock);
            var pageFactory = A.Fake<IBufferedPageFactory>();
            var gamAccessor = A.Fake<IGamAccessor>();
            var pageBuffer = new PageBuffer(pageFactory,gamMock);
            TestContext.Properties.Add("pageBuffer", pageBuffer);
        }

        private IPageBuffer pageBuffer => TestContext.Properties["pageBuffer"] as IPageBuffer;

     

        [TestMethod]
        public void RemovePageFromBuffer()
        {
         
          
        }


        [TestMethod]
        public void RemovePageFromBufferWithConcurrentCreation()
        {
           

        }


    }
}
