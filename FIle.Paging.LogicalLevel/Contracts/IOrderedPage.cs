using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;
using Pager.Classes;

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
