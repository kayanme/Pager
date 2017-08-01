using System;

namespace Pager.Classes
{
    public interface IPage : IDisposable
    {
        PageReference Reference { get; }
        void Flush();
    }

    public interface IPage<TRecordType> : IPage where TRecordType : TypedRecord, new()
    {
        double PageFullness { get; }
      

        bool AddRecord(TRecordType type);
    
      
        void FreeRecord(TRecordType record);
        TRecordType GetRecord(PageRecordReference reference);
        void StoreRecord(TRecordType record);
    }
}