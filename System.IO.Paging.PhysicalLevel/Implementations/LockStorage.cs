using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.Threading.Tasks;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    internal sealed class LockStorage : IPhysicalLocks
    {
        public bool AcqureLock(PageRecordReference record, byte lockType, out LockToken<PageRecordReference> token)
        {
            throw new NotImplementedException();
        }

        public bool AcqurePageLock(byte lockType, out LockToken<PageReference> token)
        {
            throw new NotImplementedException();
        }

        public bool ChangeLockLevel(ref LockToken<PageReference> token, byte newLevel)
        {
            throw new NotImplementedException();
        }

        public bool ChangeLockLevel(ref LockToken<PageRecordReference> token, byte newLevel)
        {
            throw new NotImplementedException();
        }

        public void ReleaseLock(LockToken<PageRecordReference> token)
        {
            throw new NotImplementedException();
        }

        public void ReleasePageLock(LockToken<PageReference> token)
        {
            throw new NotImplementedException();
        }

        public Task<LockToken<PageReference>> WaitForLockLevelChange(LockToken<PageReference> token, byte newLevel)
        {
            throw new NotImplementedException();
        }

        public Task<LockToken<PageRecordReference>> WaitForLockLevelChange(LockToken<PageRecordReference> token, byte newLevel)
        {
            throw new NotImplementedException();
        }

        public Task<LockToken<PageRecordReference>> WaitLock(PageRecordReference record, byte lockType)
        {
            throw new NotImplementedException();
        }

        public Task<LockToken<PageReference>> WaitPageLock(byte lockType)
        {
            throw new NotImplementedException();
        }
    }
}
