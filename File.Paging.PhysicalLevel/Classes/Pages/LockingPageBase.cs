using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal abstract class LockingPageBase : IPhysicalLocks 
    {
        private readonly IPhysicalLockManager<PageReference> _pageLockManager;
        private readonly IPhysicalLockManager<PageRecordReference> _pageRecordLockManager;
        private readonly LockMatrix _lockMatrix;

        protected LockingPageBase(IPhysicalLockManager<PageReference> pageLockManager,
            IPhysicalLockManager<PageRecordReference> pageRecordLockManager,
            LockMatrix lockMatrix)
        {
            _pageLockManager = pageLockManager;
            _pageRecordLockManager = pageRecordLockManager;
            _lockMatrix = lockMatrix;
        }

       

        public bool AcqurePageLock(byte lockType, out LockToken<PageReference> token)
        {
            return _pageLockManager.AcqureLock(Reference, lockType, _lockMatrix, out token);
        }

        public abstract PageReference Reference { get;  }

        public async Task<LockToken<PageReference>> WaitPageLock(byte lockType)
        {
            return await _pageLockManager.WaitLock(Reference, lockType, _lockMatrix);
        }

        public void ReleasePageLock(LockToken<PageReference> token)
        {
            _pageLockManager.ReleaseLock(token, _lockMatrix);
        }

        public bool AcqureLock(PageRecordReference record, byte lockType, out LockToken<PageRecordReference> token)
        {
            return _pageRecordLockManager.AcqureLock(record, lockType, _lockMatrix, out token);
        }

        public async Task<LockToken<PageRecordReference>> WaitLock(PageRecordReference record, byte lockType)
        {
            return await _pageRecordLockManager.WaitLock(record, lockType, _lockMatrix);
        }

        public void ReleaseLock(LockToken<PageRecordReference> token)
        {
            _pageRecordLockManager.ReleaseLock(token, _lockMatrix);
        }

        public bool ChangeLockLevel(ref LockToken<PageReference> token, byte newLevel)
        {
            return _pageLockManager.ChangeLockLevel(ref token, _lockMatrix, newLevel);
        }

        public bool ChangeLockLevel(ref LockToken<PageRecordReference> token, byte newLevel)
        {
            return _pageRecordLockManager.ChangeLockLevel(ref token, _lockMatrix, newLevel);
        }

        public async Task WaitForLockLevelChange(LockToken<PageReference> token, byte newLevel)
        {
            await _pageLockManager.WaitForLockLevelChange(token, _lockMatrix, newLevel);
        }

        public async Task WaitForLockLevelChange(LockToken<PageRecordReference> token, byte newLevel)
        {
            await _pageRecordLockManager.WaitForLockLevelChange(token, _lockMatrix, newLevel);
        }
    }
}