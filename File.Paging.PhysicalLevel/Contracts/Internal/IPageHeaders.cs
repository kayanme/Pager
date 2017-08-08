using System.Collections.Generic;

namespace File.Paging.PhysicalLevel.Contracts
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
        void SetNewRecordInfo(ushort record,ushort rSize, byte rType);
        void SwapRecords(ushort recordOne, ushort recordTwo);
        void Compact();
        int TotalUsedSize { get; }
    }
}
