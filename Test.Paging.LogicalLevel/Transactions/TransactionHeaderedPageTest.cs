using System;
using System.Transactions;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.LogicalLevel.Classes.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeItEasy;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace Test.Paging.LogicalLevel.Transactions
{
    [TestClass]
    public class TransactionHeaderedPageTest
    {
        public TestContext TestContext { get; set; }

        private TransactionHeaderResource<TestRecord> Create()
        {
            Enum.TryParse(TestContext.Properties["Isolation"] as string, out IsolationLevel isolation);
            var withLocks = bool.Parse(TestContext.Properties["LockCapable"] as string);
            var withVersions = bool.Parse((TestContext.Properties["Versioning"] ?? "false") as string);
            var hp = withLocks && withVersions ? A.Fake<IHeaderedPage<TestRecord>>(c => c.Implements<IPhysicalLocks>().Implements<IRowVersionControl>())
                : withLocks ?A.Fake<IHeaderedPage<TestRecord>>(c=>c.Implements<IPhysicalLocks>())
                    : withVersions ? A.Fake<IHeaderedPage<TestRecord>>(c => c.Implements <IRowVersionControl>())
                        : A.Fake<IHeaderedPage<TestRecord>>();

            
            var t = new TransactionHeaderResource<TestRecord>(() => { }, hp, isolation, 0, 0);
            TestContext.Properties.Add("page", hp);
            return t;
        }

        private IHeaderedPage<TestRecord> InnerPage => TestContext.Properties["page"] as IHeaderedPage<TestRecord>;
        private IPhysicalLocks Locks => TestContext.Properties["page"] as IPhysicalLocks;

        [TestProperty("Isolation", "ReadUncommitted")]
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
        [TestProperty("Versioning", "false")]
        [TestMethod]
        public void GetHeader_RC_NL()
        {
            var res = new TestRecord();
            var page = Create();
            
            A.CallTo(() => InnerPage.GetHeader()).Returns(res);
            var header = page.GetHeader();
            Assert.AreEqual(res, header);
            A.CallTo(() => InnerPage.GetHeader()).MustHaveHappened();
        }

        [TestProperty("Isolation", "ReadCommitted")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "false")]
        [TestMethod]
        public void GetAfterModifyHeader_RC_NL()
        {
            var res = new TestRecord();
            var page = Create();
            
            page.SetHeader(res);
            var header = page.GetHeader();
            Assert.AreEqual(header, res);
            
        }

        [TestProperty("Isolation", "ReadCommitted")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "false")]
        [TestMethod]
        public void ModifyHeader_RC_NL()
        {
            var res = new TestRecord();
            var page = Create();
            
            page.SetHeader(res);
            
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetHeader_RR_NL()
        {
            var page = Create();
            var header = page.GetHeader();
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifyHeader_RR_NL()
        {
            var res = new TestRecord();
            var page = Create();
            
            page.SetHeader(res);
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "false")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetHeader_S_NL()
        {
            var page = Create();
            var header = page.GetHeader();
        }

        [TestProperty("Isolation", "RepeatableRead")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "false")]
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
        [TestProperty("Versioning", "true")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetHeader_SS_NL()
        {
            var res = new TestRecord();
            var page = Create();
            A.CallTo(() => InnerPage.GetHeader()).Returns(res);

            var header = page.GetHeader();
            Assert.AreEqual(header, res);
            
        }

        [TestProperty("Isolation", "Snapshot")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "true")]
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetAfterModifyHeader_SS_NL()
        {
            var res = new TestRecord();
            var page = Create();
            
            page.SetHeader(res);
            var header = page.GetHeader();
            Assert.AreEqual(header, res);
            
        }

        [TestProperty("Isolation", "Snapshot")]
        [TestProperty("LockCapable", "false")]
        [TestProperty("Versioning", "true")]
        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void ModifyHeader_SS_NL()
        {
            var res = new TestRecord();
            var page = Create();
            
            page.SetHeader(res);
            
        }
    }
}
