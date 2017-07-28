using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager.Contracts
{
    internal interface IPageHeaders
    {
        IEnumerable<ushort> NonFreeRecords();
        short TakeNewRecord(byte rType, ushort rSize);
        bool IsRecordFree(ushort record);
        void FreeRecord(ushort record);
        ushort RecordCount { get; }    
        ushort RecordShift(ushort record);
        byte RecordType(ushort record);
        ushort RecordSize(ushort record);
    }
}
