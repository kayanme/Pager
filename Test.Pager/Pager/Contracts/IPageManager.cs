using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Classes;

namespace Pager
{

    public interface IPageManager:IDisposable
    {
        //FixedRecordTypedPage<TRecordType> CreatePage<TRecordType>(FixedRecordTypePageConfiguration<TRecordType> config) where TRecordType : TypedRecord,new();
        //ComplexRecordTypePage<TRecordType> CreatePage<TRecordType>(VariableRecordTypePageConfiguration<TRecordType> config) where TRecordType : TypedRecord, new();

        TypedPage RetrievePage(PageReference pageNum);

        TypedPage CreatePage(byte type);
        void DeletePage(PageReference page, bool ensureEmptyness);

        void GroupFlush(params TypedPage[] pages);
    }
}
