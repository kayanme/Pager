using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal abstract class PageHeadersConfiguration
    {
        public PageContentConfiguration InnerPageMap { get; set; }
        internal abstract IHeaderedPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize, byte pageType);
    }

    internal sealed class PageHeadersConfiguration<THeader> : PageHeadersConfiguration where THeader : new()
    {

        public FixedSizeRecordDeclaration<THeader> Header { get; set; }


        private ushort HeaderSize => (ushort)Header.GetSize;
        internal override IHeaderedPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize, byte pageType)
        {
            return new HeaderedPage<THeader>(accessor,
                InnerPageMap.CreatePage(headers, accessor.GetChildAccessorWithStartShift(HeaderSize), reference, pageSize - HeaderSize, pageType), reference, this);
        }
    }
}