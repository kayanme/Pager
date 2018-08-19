using System.Threading.Tasks;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
{
    internal interface IPhysicalLockManager<T>
    {
        bool AcqureLock(T lockingObject, byte lockType, LockMatrix rules, out LockToken<T> token);
        Task<LockToken<T>> WaitLock(T lockingObject, byte lockType, LockMatrix rules);
        void ReleaseLock(LockToken<T> token, LockMatrix rules);     
        bool ChangeLockLevel(ref LockToken<T> token, LockMatrix rules, byte newLevel);
        Task<LockToken<T>> WaitForLockLevelChange(LockToken<T> token, LockMatrix rules, byte newLevel);
    }
}
