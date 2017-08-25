using System.Collections.Generic;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IPageHeaders
    {
        IEnumerable<ushort> NonFreeRecords();
        short TakeNewRecord(byte rType, ushort rSize);
        bool IsRecordFree(ushort persistentRecordNum);
        void FreeRecord(ushort persistentRecordNum);
        ushort RecordCount { get; }    
        ushort RecordShift(ushort persistentRecordNum);
        byte RecordType(ushort persistentRecordNum);
        ushort RecordSize(ushort persistentRecordNum);
        void SetNewRecordInfo(ushort persistentRecordNum, ushort rSize, byte rType);

        void ApplyOrder(ushort[] recordsInOrder);
        void DropOrder(ushort persistentRecordNum);

        void Compact();
        int TotalUsedSize { get; }
    }
}
