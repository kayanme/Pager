using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Contracts
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
