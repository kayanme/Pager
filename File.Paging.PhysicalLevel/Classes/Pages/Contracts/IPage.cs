using System;
using System.Collections.Generic;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPage : IDisposable
    {
        byte RegisteredPageType { get; }
        PageReference Reference { get; }
        double PageFullness { get; }
        int UsedRecords { get; }
        int ExtentNumber { get; }
    }

    public interface IPage<TRecordType> :IDisposable where TRecordType : TypedRecord, new()
    {
              
        bool AddRecord(TRecordType type);          
        void FreeRecord(TRecordType record);
        TRecordType GetRecord(PageRecordReference reference);
        void StoreRecord(TRecordType record);
      
        IEnumerable<PageRecordReference> IterateRecords();

        void Flush();
    }
}