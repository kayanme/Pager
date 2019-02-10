using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration.Builder;
using System.Threading.Tasks;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages
{
    internal  class LockingPage :TypedPageBase, IPhysicalLocks 
    {
        private readonly ILockManager<PageReference> _pageLockManager;
        private readonly ILockManager<PageRecordReference> _pageRecordLockManager;
        private readonly LockRuleset _lockMatrix;

        public LockingPage(ILockManager<PageReference> pageLockManager,
            ILockManager<PageRecordReference> pageRecordLockManager,
            LockRuleset lockMatrix,
            PageReference reference,
            Action action):base(reference,action)
        {
            _pageLockManager = pageLockManager;
            _pageRecordLockManager = pageRecordLockManager;
            _lockMatrix = lockMatrix;
            
        }

       

        public bool AcqurePageLock(byte lockType, out LockToken<PageReference> token)
        {
            return _pageLockManager.TryAcqureLock(_reference,  _lockMatrix, lockType, out token);
        }

        private PageReference _reference { get;  }

        public async Task<LockToken<PageReference>> WaitPageLock(byte lockType)
        {
            return await _pageLockManager.WaitLock(_reference,  _lockMatrix, lockType);
        }

        public void ReleasePageLock(LockToken<PageReference> token)
        {
            _pageLockManager.ReleaseLock(token, _lockMatrix);
        }

        public bool AcqureLock(PageRecordReference record, byte lockType, out LockToken<PageRecordReference> token)
        {
            return _pageRecordLockManager.TryAcqureLock(record,  _lockMatrix, lockType, out token);
        }

        public async Task<LockToken<PageRecordReference>> WaitLock(PageRecordReference record, byte lockType)
        {
            return await _pageRecordLockManager.WaitLock(record, _lockMatrix, lockType);
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

        public async Task<LockToken<PageReference>> WaitForLockLevelChange(LockToken<PageReference> token, byte newLevel)
        {
            return await _pageLockManager.WaitForLockLevelChange(token, _lockMatrix, newLevel);
        }

        public async Task<LockToken<PageRecordReference>> WaitForLockLevelChange(LockToken<PageRecordReference> token, byte newLevel)
        {
           return await _pageRecordLockManager.WaitForLockLevelChange(token, _lockMatrix, newLevel);
        }
    }
}