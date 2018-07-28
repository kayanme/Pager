using System;
using System.Threading.Tasks;
using System.Transactions;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Classes.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using TechTalk.SpecFlow;

namespace Test.Paging.LogicalLevel.Transactions
{
    [Binding]
    public class ReadCommitedTransactionFlowSteps
    {
        [Given(@"configured transactable page with read lock naming '(.*)' and write lock naming '(.*)'")]
        public void GivenConfiguredTransactablePageWithReadLockNamingAndWriteLockNaming(byte p0, byte p1)
        {
             ScenarioContext.Current.Add("readlock",p0);
             ScenarioContext.Current.Add("writelock", p1);
            
        }
        
        [Given(@"transaction with isolation level '(.*)'")]
        public void GivenTransactionWithIsolationLevel(string p0)
        {
            ScenarioContext.Current.Add("isolationlevel", Enum.Parse(typeof(IsolationLevel), p0));
        }
        private TestRecord th => (TestRecord)ScenarioContext.Current["testheader"];
        private byte readlock => (byte)ScenarioContext.Current["readlock"];
        private byte writelock => (byte)ScenarioContext.Current["writelock"];
        private IHeaderedPage<TestRecord> pp => ScenarioContext.Current["pp"] as IHeaderedPage<TestRecord>;
        private IPhysicalLocks locks => ScenarioContext.Current["pp"] as IPhysicalLocks;
        private MockRepository mocks => (MockRepository)ScenarioContext.Current["mr"];
        private LockToken<PageReference> heldpagelock => (LockToken<PageReference>)ScenarioContext.Current["heldpagelock"];
        private TransactionHeaderResource<TestRecord> hp => (TransactionHeaderResource < TestRecord > )ScenarioContext.Current["hp"];

        [Given(@"we have a headered page")]
        public void GivenWeHaveAHeaderedPage()
        {
         
            var il = (IsolationLevel)ScenarioContext.Current["isolationlevel"];
            var hp = new TransactionHeaderResource<TestRecord>(() => { }, pp, il,readlock,writelock);
            ScenarioContext.Current.Add("hp",hp);
            var th = new TestRecord();
            ScenarioContext.Current.Add("testheader", th);
        }
        
        [When(@"we get a header")]
        public void WhenWeGetAHeader()
        {
            var hp = ScenarioContext.Current["hp"] as TransactionHeaderResource<TestRecord>;
            ScenarioContext.Current.Add("worktask",new Task<TestRecord>(()=>hp.GetHeader()));
        }
        
        [When(@"we modify a header")]
        public void WhenWeModifyAHeader()
        {
            var hp = ScenarioContext.Current["hp"] as TransactionHeaderResource<TestRecord>;
            pp.Expect(k => k.GetHeader()).Return(default(TestRecord));
            ScenarioContext.Current.Add("worktask", new Task(() => hp.SetHeader(th)));
        }
        
        [Then(@"we expect acquiring read lock on page")]
        public void ThenWeExpectAcquiringReadLockOnPage()
        {
            var holdinglock = new LockToken<PageReference>();
            locks.Expect(k => k.WaitPageLock(readlock)).Return(new Task<LockToken<PageReference>>(() => holdinglock));
            ScenarioContext.Current.Add("heldpagelock", holdinglock);
        }
        
        [Then(@"we expect read a header from physical page")]
        public void ThenWeExpectReadAHeaderFromPhysicalPage()
        {
            pp.Expect(k => k.GetHeader()).Return(th);
        }
        
        [Then(@"we expect release read lock on page")]
        public void ThenWeExpectReleaseReadLockOnPage()
        {
            locks.Expect(k => k.ReleasePageLock(heldpagelock));
        }
        
        [Then(@"finally return a result from physical page")]
        public void ThenFinallyReturnAResultFromPhysicalPage()
        {
            mocks.ReplayAll();
            var task = ScenarioContext.Current["worktask"] as Task<TestRecord>;
            task.RunSynchronously();
            Assert.AreEqual(th,task.Result);
            mocks.VerifyAll();
        }
        
        [Then(@"we expect acquiring write lock on page")]
        public void ThenWeExpectAcquiringWriteLockOnPage()
        {
            var holdinglock = new LockToken<PageReference>();
            locks.Expect(k => k.WaitPageLock(writelock)).Return(new Task<LockToken<PageReference>>(() => holdinglock));
            ScenarioContext.Current.Add("heldpagelock", holdinglock);
        }

       
        [Then(@"we commit the transaction")]
        public void ThenWeCommitTheTransaction()
        {
         
           
            switch (ScenarioContext.Current["worktask"])
            {
                case Task<TestRecord> t:
                     t.ContinueWith(t2 =>
                    {
                        hp.Prepare(null);
                        hp.Commit(null);
                        return t2.Result;
                    });
                    break;
                case Task t:
                   t.ContinueWith(t2 =>
                   {
                       
                        hp.Prepare(null);
                        hp.Commit(null);                       
                    });
                    break;
            
            }
          
        }
        
        [Then(@"expect header modified to physical page")]
        public void ThenExpectHeaderModifiedToPhysicalPage()
        {
            pp.Expect(k => k.ModifyHeader(th));
        }
        
        [Then(@"we expect release write lock on page")]
        public void ThenWeExpectReleaseWriteLockOnPage()
        {
          locks.Expect(k=>k.ReleasePageLock(heldpagelock));
        }
        
        [Then(@"so be it")]
        public void ThenSoBeIt()
        {
          mocks.ReplayAll();
            var task = ScenarioContext.Current["worktask"] as Task;
            task.RunSynchronously();       
            mocks.VerifyAll();
        }
    }
}
