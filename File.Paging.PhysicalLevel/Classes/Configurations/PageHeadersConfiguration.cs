using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal abstract class PageHeadersConfiguration
    {
        public PageContentConfiguration InnerPageMap { get; set; }
        internal abstract IHeaderedPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize, byte pageType);
    }

    internal sealed class PageHeadersConfiguration<TRecord,THeader> : PageHeadersConfiguration where THeader : new() where TRecord:TypedRecord,new()
    {

        public FixedSizeRecordDeclaration<THeader> Header { get; set; }


        private ushort HeaderSize => (ushort)Header.GetSize;
        internal override IHeaderedPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize, byte pageType)
        {
           var innerPage = InnerPageMap.CreatePage(headers, accessor.GetChildAccessorWithStartShift(HeaderSize), reference,
                pageSize - HeaderSize, pageType) as IPage<TRecord>;
            Debug.Assert(innerPage != null,"innerPage != null");
            return new HeaderedPage<TRecord, THeader>(headers,accessor, innerPage, reference, this);
        }
    }
}