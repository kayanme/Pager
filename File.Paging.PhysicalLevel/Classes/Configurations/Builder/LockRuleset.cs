using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public abstract class LockRuleset
    {
        public abstract byte GetLockLevelCount();

        public abstract bool AreShared(byte heldLockType, byte acquiringLockType);
    }

    public sealed class ReaderWriterLockRuleset:LockRuleset
    {
        public override byte GetLockLevelCount() => 2;

        public override bool AreShared(byte heldLockType, byte acquiringLockType)
        {
            if (heldLockType == acquiringLockType && acquiringLockType == 0)
                return true;
            return false;
            
        }
    }
}
