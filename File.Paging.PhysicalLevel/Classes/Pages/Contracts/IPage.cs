using System;
using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPage<TRecordType> :IDisposable where TRecordType : struct
    {

        TypedRecord<TRecordType> AddRecord(TRecordType type);          
        void FreeRecord(TypedRecord<TRecordType> record);
        TypedRecord<TRecordType> GetRecord(PageRecordReference reference);
        void StoreRecord(TypedRecord<TRecordType> record);
        IEnumerable<TypedRecord<TRecordType>> GetRecordRange(PageRecordReference start,PageRecordReference end); 
        IEnumerable<TypedRecord<TRecordType>> IterateRecords();
        

        void Flush();
    }
}