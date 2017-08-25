using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Pager.Pages
{
   [TestClass]
    public class FixedSizePageTest
    {
        public TestContext TestContext { get; set; }
        private IPage<TestRecord> Create()
        {
            var mp = new MockRepository();
            var headers = mp.StrictMock<IPageHeaders>();
            var access = mp.StrictMock<IPageAccessor>();
            var config = new FixedRecordTypePageConfiguration<TestRecord>()
            {
                RecordMap = new FixedSizeRecordDeclaration<TestRecord>((t, b) => t.FillByteArray(b), (b, t) => t.FillFromByteArray(b), 4)
            };
            var page = new FixedRecordTypedPage<TestRecord>(headers, access, new PageReference(0), 4096, config, 1);
            TestContext.Properties.Add("headers", headers);
            TestContext.Properties.Add("access", access);
            return page;
        }

        private IPageHeaders Headers => TestContext.Properties["headers"] as IPageHeaders;
        private IPageAccessor Access => TestContext.Properties["access"] as IPageAccessor;

        [TestMethod]
        public void AddRecord()
        {
            var page = Create();
            Headers.Expect(k => k.TakeNewRecord(1, 4)).Return(0);
            Headers.Expect(k => k.RecordShift(0)).Return(2).Repeat.Any();
            Headers.Expect(k => k.RecordSize(0)).Return(4);
            Access.Expect(k => k.GetByteArray(2, 4)).Return(new byte[4]);
            Access.Expect(k => k.SetByteArray(new byte[] { 0,0,0,4}, 2, 4));
            Headers.Replay();
            Access.Replay();
            var res = page.AddRecord(new TestRecord { Value = 4 });
            Assert.IsTrue(res);
            Headers.VerifyAllExpectations();
            Access.VerifyAllExpectations();
        }

        [TestMethod]
        public void AddRecord_NoSpace()
        {
            var page = Create();
            Headers.Expect(k => k.TakeNewRecord(1, 4)).Return(-1);
           
            Headers.Replay();
            Access.Replay();
            var res = page.AddRecord(new TestRecord { Value = 4 });
            Assert.IsFalse(res);
            Headers.VerifyAllExpectations();
            Access.VerifyAllExpectations();
        }

    }
}
