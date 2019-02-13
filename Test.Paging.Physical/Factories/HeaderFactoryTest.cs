using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Implementations.Headers;
using System.Text;

namespace Test.Paging.PhysicalLevel.Factories
{
    [TestClass]
    public class HeaderFactoryTest
    {

        private IHeaderFactory HeaderFact()
        {
            return new HeaderFactory();
        }

        [TestMethod]
        public void ImagePageTest()
        {
            var accessor = A.Fake<IPageAccessor>();
            var pageContent = A.Fake<PageContentConfiguration>();
            A.CallTo(() => accessor.PageSize).Returns(10);
            A.CallTo(() => pageContent.ReturnHeaderInfo()).Returns(new HeaderInfo(true, false, 10));
            var headers = HeaderFact().CreateHeaders(pageContent, accessor);
            Assert.IsInstanceOfType(headers,typeof(ImagePageHeader));
        }
    }
}
