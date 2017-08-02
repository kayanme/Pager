using System;
using System.Collections.Generic;

namespace Pager.Classes
{
    public interface IPage : IDisposable
    {
        byte RegisteredPageType { get; }
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
        void SwapRecords(PageRecordReference record1, PageRecordReference record2);
        IEnumerable<TRecordType> IterateRecords();
    }
}