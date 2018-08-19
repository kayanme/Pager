using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Exceptions;
using System.IO.Paging.PhysicalLevel.Implementations;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Classes
{
    public struct LockToken<T>
    {
        public readonly  byte LockLevel;
        internal readonly T LockedObject;
        private readonly IPhysicalLockManager<T> _holder;
        private readonly LockMatrix _matrix;
        private int _isReleased;
        internal LockToken(byte lockLevel, T lockedObject, IPhysicalLockManager<T> holder, LockMatrix matrix,int sharedLockCount)
        {
            LockLevel = lockLevel;
            LockedObject = lockedObject;
            _holder = holder;
            _matrix = matrix;
            _isReleased = 0;
            SharedLockCount = sharedLockCount;
        }
        public readonly int SharedLockCount;
        internal string TokenState => $"{(_holder as LockManager<T>)}";

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case LockToken<T> t: return t.LockLevel == LockLevel && LockedObject.Equals(t.LockedObject);
                default: return false;
            }
        }


        public override int GetHashCode()
        {
            return LockLevel ^ LockedObject.GetHashCode();
        }

        public void Release()
        {
            if (Interlocked.CompareExchange(ref _isReleased, 1, 0) == 1)
                throw new LockTokenWasAlreadyReleasedException();
            _holder.ReleaseLock(this,_matrix);            
        }
    }
}
