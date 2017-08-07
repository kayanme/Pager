using System;
using System.Transactions;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Classes.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Paging.LogicalLevel.Transactions
{
    [TestClass]
    public class TransactionHeaderedPageTest
    {
        public TestContext TestContext { get; set; }

        private TransactionHeaderResource<TestRecord> Create()
        {            
            Enum.TryParse(TestContext.Properties["Isolation"] as string,out IsolationLevel isolation);
            var withLocks = bool.Parse(TestContext.Properties["LockCapable"] as string);
            var withVersions = bool.Parse((TestContext.Properties["Versioning"]??"false") as string);
            var mp = new MockRepository();
            var hp = withLocks && withVersions ? mp.StrictMultiMock<IHeaderedPage<TestRecord>>(typeof(IPhysicalLocks), typeof(IRowVersionControl))
                               : withLocks ? mp.StrictMultiMock<IHeaderedPage<TestRecord>>(typeof(IPhysicalLocks))
                               : withVersions ? mp.StrictMultiMock<IHeaderedPage<TestRecord>>(typeof(IRowVersionControl))
                               : mp.StrictMock<IHeaderedPage<TestRecord>>();
            var t = new TransactionHeaderResource<TestRecord>(() => { }, hp, isolation,0,0);
            TestContext.Properties.Add("page", hp);
            return t;
        }

        private IHeaderedPage<TestRecord> InnerPage => TestContext.Properties["page"] as IHeaderedPage<TestRecord>;
        private IPhysicalLocks Locks => TestContext.Properties["page"] as IPhysicalLocks;

        [TestProperty("Isolation","ReadUncommitted")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetHeader_RU_NL()
        {
            var page = Create();
            var header = page.GetHeader();            
        }

        [TestProperty("Isolation", "ReadCommitted")]
        [TestProperty("LockCapable", "false")]
        [TestMethod]
        public void GetHeader_RC_NL()
        {
            var res = new TestRecord();
            var page = Create();
            InnerPage.Expect(k => k.GetHeader()).Return(res);
            InnerPage.Replay();
            var header = page.GetHeader();
            Assert.AreEqual(res, header);
            InnerPage.VerifyAllExpectations();
        }

        [TestProperty("Isolation", "ReadCommitted")]
        [TestProperty("LockCapable", "false")]
        [TestMethod]
        public void GetAfterModifyHeader_RC_NL()
        {
            var res = new TestRecord();
            var page = Create();            
            InnerPage.Replay();       
            page.SetHeader(res);
            var header = page.GetHeader();
            Assert.AreEqual(header, res);
            InnerPage.VerifyAllExpectations();
        }

        [TestProperty("Isolation", "ReadCommitted")]
        [TestProperty("LockCapable", "false")]
        [TestMethod]
        public void ModifyHeader_RC_NL()
        {
            var res = new TestRecord();
            var page = Create();
            InnerPage.Replay();
            page.SetHeader(res);
            InnerPage.VerifyAllExpectations();
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetHeader_RR_NL()
        {
            var page = Create();
            var header = page.GetHeader();
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifyHeader_RR_NL()
        {
            var res = new TestRecord();
            var page = Create();
            InnerPage.Replay();      
            page.SetHeader(res);            
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetHeader_S_NL()
        {
            var page = Create();
            var header = page.GetHeader();
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifyHeader_S_NL()
        {
            var res = new TestRecord();
            var page = Create();
            page.SetHeader(res);
        }


        [TestProperty("Isolation", "Snapshot")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning","true")]
        [TestMethod]
        public void GetHeader_SS_NL()
        {
            var res = new TestRecord();
            var page = Create();
            InnerPage.Expect(k => k.GetHeader()).Return(res);
            InnerPage.Replay();
            var header = page.GetHeader();
            Assert.AreEqual(header, res);
            InnerPage.VerifyAllExpectations();
        }

        [TestProperty("Isolation", "Snapshot")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "true")]
        [TestMethod]
        public void GetAfterModifyHeader_SS_NL()
        {
            var res = new TestRecord();
            var page = Create();            
            InnerPage.Replay();
            
            page.SetHeader(res);
            var header = page.GetHeader();
            Assert.AreEqual(header, res);
            InnerPage.VerifyAllExpectations();
        }

        [TestProperty("Isolation", "Snapshot")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "true")]
        [TestMethod]
        public void ModifyHeader_SS_NL()
        {
            var res = new TestRecord();
            var page = Create();
            InnerPage.Replay();            
            page.SetHeader(res);
            InnerPage.VerifyAllExpectations();
        }
    }
}
