using System.Collections.Generic;

namespace System.IO.Paging.PhysicalLevel.Contracts.Internal
{
    internal interface IPageHeaders
    {
        IEnumerable<ushort> NonFreeRecords();
        short TakeNewRecord(ushort rSize);
        bool IsRecordFree(ushort persistentRecordNum);
        void FreeRecord(ushort persistentRecordNum);
        ushort RecordCount { get; }    
        ushort RecordShift(ushort persistentRecordNum);
        
        ushort RecordSize(ushort persistentRecordNum);
        void SetNewRecordInfo(ushort persistentRecordNum, ushort rSize);

        void ApplyOrder(ushort[] recordsInOrder);
        void DropOrder(ushort persistentRecordNum);

        void Compact();
        int TotalUsedSize { get; }
    }
}
