using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Contracts.Internal
{
    internal interface IRecordAcquirer<TRecord> where TRecord : struct
    {
        TRecord GetRecord(ushort offset, ushort size);
        void SetRecord(ushort offset, ushort size, TRecord record);
    }
}
