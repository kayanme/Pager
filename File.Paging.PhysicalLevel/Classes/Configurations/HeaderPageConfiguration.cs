using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Classes;
using Pager.Contracts;

namespace Pager.Classes
{
    public abstract class HeaderPageConfiguration
    {
        public PageConfiguration InnerPageMap { get; set; }
        internal abstract IHeaderedPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize, byte pageType);
    }
    public sealed class HeaderPageConfiguration<THeader>: HeaderPageConfiguration where THeader : new()
    {
       
        public FixedSizeRecordDeclaration<THeader> Header { get; set; }

       
        private ushort HeaderSize => (ushort)Header.GetSize;
        internal override IHeaderedPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize,byte pageType) 
        {
            return new HeaderedPage<THeader>(accessor,
                InnerPageMap.CreatePage(headers, accessor.GetChildAccessorWithStartShift(HeaderSize), reference, pageSize - HeaderSize, pageType), reference,this);
        }
    }
}
