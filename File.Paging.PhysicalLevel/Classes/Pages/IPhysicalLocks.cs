using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPhysicalLocks
    {     
        bool AcqurePageLock(int lockType,out PageLockToken token);
        Task<PageLockToken> WaitPageLock(int lockType);
        void ReleasePageLock(PageLockToken token);

        bool AcqureLock(PageRecordReference record, int lockType, out LockToken token);
        Task<LockToken> WaitLock(PageRecordReference record, int lockType);
        void ReleaseLock(LockToken token);
    }
   
}
