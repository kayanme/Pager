using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace File.Paging.PhysicalLevel.Implementations
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
