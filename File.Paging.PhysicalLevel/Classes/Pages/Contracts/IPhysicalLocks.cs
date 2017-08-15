using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPhysicalLocks
    {     
        bool AcqurePageLock(int lockType,out LockToken<PageReference> token);
        Task<LockToken<PageReference>> WaitPageLock(int lockType);
        void ReleasePageLock(LockToken<PageReference> token);

        bool AcqureLock(PageRecordReference record, int lockType, out LockToken<PageRecordReference> token);
        Task<LockToken<PageRecordReference>> WaitLock(PageRecordReference record, int lockType);
        void ReleaseLock(LockToken<PageRecordReference> token);

        bool ChangeLockLevel<T>(LockToken<T> token, int newLevel);
        Task WaitForLockLevelChange<T>(LockToken<T> token, int newLevel);
    }
   
}
