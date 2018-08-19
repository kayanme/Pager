using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.LogicalLevel.Contracts
{
    public interface IOrderedPage<TRecord,TKey>:IPage<TRecord> where TRecord:struct
    {

        TypedRecord<TRecord> FindByKey(TKey key);
        TypedRecord<TRecord>[] FindInKeyRange(TKey start, TKey end);
        TypedRecord<TRecord> FindTheMostLesser(TKey key,bool orEqual);
        TypedRecord<TRecord> FindTheLessGreater(TKey key, bool orEqual);
#if DEBUG
        TypedRecord<TRecord> TestGetRecord(PageRecordReference extRef);
#endif
    }
}
