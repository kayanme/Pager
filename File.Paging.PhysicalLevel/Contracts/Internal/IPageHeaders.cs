using System.Collections.Generic;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IPageHeaders
    {
        IEnumerable<ushort> NonFreeRecords();
        Task<short> TakeNewRecord(byte rType, ushort rSize);
        Task<bool> IsRecordFree(ushort record);
        Task FreeRecord(ushort record);
        ushort RecordCount { get; }    
        ushort RecordShift(ushort record);
        byte RecordType(ushort record);
        ushort RecordSize(ushort record);
        Task SetNewRecordInfo(ushort record,ushort rSize, byte rType);
        Task SwapRecords(ushort recordOne, ushort recordTwo);
        Task Compact();
        int TotalUsedSize { get; }
    }
}
