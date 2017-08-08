using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Paging.LogicalLevel
{
    [TestClass]
    public class VirtualHeapPageTest
    {
        public TestContext TestContext { get; set; }
        private IPage<TestRecord> CreatePage()
        {
            var mr = new MockRepository();
            var physManager = mr.StrictMock<IPageManager>();
            var tp = new VirtualContiniousPage<TestRecord,LinkedPageHeaderBase>(physManager,2,1);
        }
    }
}
