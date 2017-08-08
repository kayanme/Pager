using System;
using System.Collections.Generic;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPage : IDisposable
    {
        byte RegisteredPageType { get; }
        PageReference Reference { get; }
        double PageFullness { get; }
        
    }

    public interface IPage<TRecordType> : IPage where TRecordType : TypedRecord, new()
    {
              
        bool AddRecord(TRecordType type);          
        void FreeRecord(TRecordType record);
        TRecordType GetRecord(PageRecordReference reference);
        void StoreRecord(TRecordType record);
      
        IEnumerable<TRecordType> IterateRecords();
    }
}