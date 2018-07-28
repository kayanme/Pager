using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPhysicalLocks
    {     
        bool AcqurePageLock(byte lockType, out LockToken<PageReference> token);
        Task<LockToken<PageReference>> WaitPageLock(byte lockType);
        void ReleasePageLock(LockToken<PageReference> token);

        bool AcqureLock(PageRecordReference record, byte lockType, out LockToken<PageRecordReference> token);
        Task<LockToken<PageRecordReference>> WaitLock(PageRecordReference record, byte lockType);
        void ReleaseLock(LockToken<PageRecordReference> token);

        bool ChangeLockLevel(ref LockToken<PageReference> token, byte newLevel);
        bool ChangeLockLevel(ref LockToken<PageRecordReference> token, byte newLevel);
        Task<LockToken<PageReference>> WaitForLockLevelChange(LockToken<PageReference> token, byte newLevel);
        Task<LockToken<PageRecordReference>> WaitForLockLevelChange(LockToken<PageRecordReference> token, byte newLevel);
    }
   
}
