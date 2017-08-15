using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace File.Paging.PhysicalLevel.Classes.Pages.Contracts
{
    internal interface IPhysicalLockManager<T>
    {
        bool AcqureLock(T lockingObject, byte lockType, LockMatrix rules, out LockToken<T> token);
        Task<LockToken<T>> WaitLock(T lockingObject, byte lockType, LockMatrix rules);
        void ReleaseLock(LockToken<T> token, LockMatrix rules);     
        bool ChangeLockLevel(LockToken<T> token, LockMatrix rules, byte newLevel);
        Task WaitForLockLevelChange(LockToken<T> token, LockMatrix rules, byte newLevel);
    }
}
