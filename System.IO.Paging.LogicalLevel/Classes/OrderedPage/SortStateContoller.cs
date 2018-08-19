using System.Threading;

namespace System.IO.Paging.LogicalLevel.Classes.OrderedPage
{
    internal class SortStateContoller
    {
        public bool IsSorted;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        public void AcquireReadLock()
        {
            _lock.EnterReadLock();
        }

        public void ReleaseReadLock()
        {
            _lock.ExitReadLock();
        }
        public void AcquireSortLock()
        {
            _lock.EnterWriteLock();
        }

        public void ReleaseSortLock()
        {
            _lock.ExitWriteLock();
        }
    }
}
