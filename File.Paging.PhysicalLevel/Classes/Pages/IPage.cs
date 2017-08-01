using System;

namespace Pager.Classes
{
    public interface IPage : IDisposable
    {
        PageReference Reference { get; }
        double PageFullness { get; }
        void Flush();
    }

    public interface IPage<TRecordType> : IPage where TRecordType : TypedRecord, new()
    {
              
        bool AddRecord(TRecordType type);          
        void FreeRecord(TRecordType record);
        TRecordType GetRecord(PageRecordReference reference);
        void StoreRecord(TRecordType record);
        void SwapRecords(TRecordType record1, TRecordType record2);
    }
}