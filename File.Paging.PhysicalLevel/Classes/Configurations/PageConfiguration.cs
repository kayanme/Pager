using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Contracts;

namespace Pager.Classes
{
    public abstract class PageConfiguration
    {
        internal abstract Type RecordType { get; }

        internal abstract IPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize);

        internal abstract IPageHeaders CreateHeaders(IPageAccessor accessor,ushort initShift);
    }
}
