using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{

    public interface IPageManager:IDisposable
    {
        FixedRecordTypedPage<TRecordType> CreatePage<TRecordType>() where TRecordType : TypedRecord,new();
        FixedRecordTypedPage<TRecordType> RetrievePage<TRecordType>(PageReference pageNum)
            where TRecordType : TypedRecord,new();
       

        void DeletePage(PageReference page, bool ensureEmptyness);

        void GroupFlush(params TypedPage[] pages);
    }
}
