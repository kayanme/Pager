using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    [Flags]
    public enum ConsistencyAbilities { None = 0, PhysicalLocks =1,RowVersionCheckOnUpdate=2,PageChecksumProtection=4,RowChecksumProtection=8}
}