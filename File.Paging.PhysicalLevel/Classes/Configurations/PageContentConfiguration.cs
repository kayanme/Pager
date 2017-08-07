using System;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal abstract class PageContentConfiguration
    {
        public ConsistencyConfiguration ConsistencyConfiguration;

        internal abstract Type RecordType { get; }

        internal abstract IPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize,byte pageType);

        internal abstract IPageHeaders CreateHeaders(IPageAccessor accessor,ushort initShift);

        public abstract void Verify();
    }
}
