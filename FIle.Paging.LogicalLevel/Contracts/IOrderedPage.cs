using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Contracts
{
    public interface IOrderedPage<TRecord>:IPage<TRecord> where TRecord:TypedRecord,new()
    {
        TRecord First();
        TRecord Last();
#if DEBUG
        TRecord TestGetRecord(PageRecordReference extRef);
#endif
    }
}
