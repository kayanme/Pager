using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
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