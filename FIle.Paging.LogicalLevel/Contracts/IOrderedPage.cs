using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Contracts
{
    public interface IOrderedPage<TRecord,TKey>:IPage<TRecord> where TRecord:struct
    {

        TypedRecord<TRecord> FindByKey(TKey key);
#if DEBUG
        TypedRecord<TRecord> TestGetRecord(PageRecordReference extRef);
#endif
    }
}
