using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace File.Paging.PhysicalLevel.Classes
{
    public struct LockToken<T>
    {
        public readonly  byte LockLevel;
        internal readonly T LockedObject;
        private readonly IPhysicalLockManager<T> _holder;
        private readonly LockMatrix _matrix;
        internal LockToken(byte lockLevel, T lockedObject, IPhysicalLockManager<T> holder, LockMatrix matrix)
        {
            LockLevel = lockLevel;
            LockedObject = lockedObject;
            _holder = holder;
            _matrix = matrix;
        }

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
            _holder.ReleaseLock(this,_matrix);
        }
    }
}
