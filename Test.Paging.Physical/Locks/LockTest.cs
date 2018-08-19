using System.Threading.Tasks;
using FakeItEasy;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Paging.PhysicalLevel.Locks
{
    [TestClass]
    public class LockTest
    {
        public TestContext TestContext { get; set; }
        private IPhysicalLockManager<int> CreateLock(bool readerWriteLockScheme)
        {

            var rules = A.Fake<LockRuleset>();
            if (!readerWriteLockScheme)
            {
                A.CallTo(() => rules.GetLockLevelCount()).Returns<byte>(1);
                A.CallTo(() => rules.AreShared(0, 0)).Returns(false);
            }
            else
            {
                A.CallTo(() => rules.GetLockLevelCount()).Returns<byte>(2);
                A.CallTo(() => rules.AreShared(0, 0)).Returns(true);
                A.CallTo(() => rules.AreShared(1, 0)).Returns(false);
                A.CallTo(() => rules.AreShared(0, 1)).Returns(false);
                A.CallTo(() => rules.AreShared(1, 1)).Returns(false);
            }

            matrix = new LockMatrix(rules);
            return new LockManager<int>();
        }


        private LockMatrix matrix
        {
            get => TestContext.Properties["rules"] as LockMatrix;
            set => TestContext.Properties.Add("rules", value);
        }

        [TestMethod]
        public void AcquireRecordLock()
        {
            var manager = CreateLock(false);

            Assert.IsTrue(manager.AcqureLock(1, 0, matrix, out var token));
            Assert.AreEqual(0, token.LockLevel);
            Assert.AreEqual(1, token.LockedObject);
        }


        [TestMethod]
        public void AcquireRecordLockAndCheckItLocked()
        {
            var manager = CreateLock(false);

            manager.AcqureLock(1, 0, matrix, out var token);
            Assert.IsFalse(manager.AcqureLock(1, 0, matrix, out var _));
        }

        [TestMethod]
        public void AcquireRecordLockAndCheckItNotLockedForAnotherRecord()
        {
            var manager = CreateLock(false);

            manager.AcqureLock(1, 0, matrix, out var token);
            Assert.IsTrue(manager.AcqureLock(2, 0, matrix, out var _));
        }

        [TestMethod]
        public void AcquireRecordLock_AndRelease_AndReacquire()
        {
            var manager = CreateLock(false);

            manager.AcqureLock(1, 0, matrix, out var token);
            manager.ReleaseLock(token, matrix);
            Assert.IsTrue(manager.AcqureLock(1, 0, matrix, out token));
        }


        [TestMethod]
        public void AcquireRecordLock_AndRelease_AndReacquire_Alt()
        {
            var manager = CreateLock(false);

            manager.AcqureLock(1, 0, matrix, out var token);
            token.Release();
            Assert.IsTrue(manager.AcqureLock(1, 0, matrix, out token));
        }

        [TestMethod]
        public async Task AcquireRecordLock_AndRelease_AndWaitForIt()
        {
            var manager = CreateLock(false);

            manager.AcqureLock(1, 0, matrix, out var token);
            token.Release();
            token = await manager.WaitLock(1,0,matrix);
            Assert.AreEqual(1,token.LockedObject);
            Assert.AreEqual(0, token.LockLevel);
        }

        [TestMethod]
        public void AcquireSharedRecordLockAndCheckItNotLocked()
        {
            var manager = CreateLock(true);

            manager.AcqureLock(1, 0, matrix, out _);
            Assert.IsTrue(manager.AcqureLock(1, 0, matrix, out _));
        }

        [TestMethod]
        public void AcquireSharedRecordTwoTimesLockAndCheckItLockedForNonSharedAfterOneRelease_AndNotLockedAfterSecond()
        {
            var manager = CreateLock(true);

            manager.AcqureLock(1, 0, matrix, out var token);
            manager.AcqureLock(1, 0, matrix, out var token2);
            token.Release();
            Assert.IsFalse(manager.AcqureLock(1, 1, matrix, out _));
            token2.Release();
            Assert.IsTrue(manager.AcqureLock(1, 1, matrix, out _));

        }

        [TestMethod]
        public void AcquireSharedRecordLockAndCheckItLockedForNonShared()
        {
            var manager = CreateLock(true);

            manager.AcqureLock(1, 0, matrix, out var _);
            Assert.IsFalse(manager.AcqureLock(1, 1, matrix, out var _));
        }

        [TestMethod]
        public void AcquireSharedRecordLock_ThenRelease_AndCheckItNotLockedForNonShared()
        {
            var manager = CreateLock(true);

            manager.AcqureLock(1, 0, matrix, out var token);
            token.Release();
            Assert.IsTrue(manager.AcqureLock(1, 1, matrix, out var _));
        }

        [TestMethod]
        public void WaitForRecordLock()
        {
            var manager = CreateLock(false);

            var token = manager.WaitLock(1, 0, matrix).Result;
            Assert.AreEqual(0, token.LockLevel);
            Assert.AreEqual(1, token.LockedObject);
        }


        [TestMethod]
        public void WaitForRecordLockAndCheckItLocked()
        {
            var manager = CreateLock(false);

            var task = manager.WaitLock(1, 0, matrix);
            task.Wait();
            Assert.IsFalse(manager.AcqureLock(1, 0, matrix, out var _));
        }

        [TestMethod]
        public void AcquireLockAndCheckThatItUnlocksForWaiterAfterRelease()
        {
            var manager = CreateLock(false);

            manager.AcqureLock(1, 0, matrix, out var token);
            bool lockAcquired = false;
            var task = manager.WaitLock(1, 0, matrix);
            var t2 = task.ContinueWith(t =>
            {
                token = t.Result;
                lockAcquired = true;
            });

            Assert.IsFalse(lockAcquired);
            manager.ReleaseLock(token, matrix);

            t2.Wait(1000);
            Assert.IsTrue(lockAcquired);
            Assert.AreEqual(0, token.LockLevel);
            Assert.AreEqual(1, token.LockedObject);
        }

        [TestMethod]
        public async Task WaitForSharedRecordLockAndCheckItNotLocked()
        {
            var manager = CreateLock(true);

            await manager.WaitLock(1, 0, matrix);
            Assert.IsTrue(manager.AcqureLock(1, 0, matrix, out var _));
        }

        [TestMethod]
        public void TakeSharedRecordLock_AndWaitForNonShared()
        {
            var manager = CreateLock(true);
            manager.AcqureLock(1, 0, matrix, out var token);
            var acquired = false;
            var t = manager.WaitLock(1, 1, matrix);
            var t2 = t.ContinueWith(_ => acquired = true);

            Assert.IsFalse(acquired);
            token.Release();
            t2.Wait(1000);
            Assert.IsTrue(acquired);
        }

        [TestMethod]
        public void TakeSharedLock_ReleaseIt_AndWaitForNonShared()
        {
            var manager = CreateLock(true);
            manager.AcqureLock(1, 0, matrix, out var token);
            token.Release();
            var acquired = false;
            var t = manager.WaitLock(1, 1, matrix);
            var t2 = t.ContinueWith(_ => acquired = true);

            Assert.IsFalse(acquired);
            
            t2.Wait(1000);
            Assert.IsTrue(acquired);
        }

        [TestMethod]
        public void ChainOFSharedAndNonShared()
        {
            var manager = CreateLock(true);
            manager.AcqureLock(1, 0, matrix, out var token);
            token.Release();
            manager.AcqureLock(1, 1, matrix,out token);
            token.Release();
            manager.AcqureLock(1, 0, matrix, out token);
            token.Release();         
            var acquired =  manager.AcqureLock(1, 1, matrix,out token);
         
            Assert.IsTrue(acquired);
        }


        [TestMethod]
        public void TakeTwoSharedLocks_AndWaitForNonShared()
        {
            var manager = CreateLock(true);

            manager.AcqureLock(1, 0, matrix, out var token);
            manager.AcqureLock(1, 0, matrix, out var token2);
            var acquired = false;
            var t = manager.WaitLock(1, 1, matrix);
            var t2 = t.ContinueWith(_ => acquired = true);

            Assert.IsFalse(acquired);
            token.Release();
            Assert.IsFalse(t2.Wait(50));
            token2.Release();
            t2.Wait(50);
            Assert.IsTrue(acquired);
        }
    }
}
