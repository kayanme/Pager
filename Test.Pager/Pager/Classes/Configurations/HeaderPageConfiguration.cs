using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Classes;
using Pager.Contracts;

namespace Pager.Classes
{
    public sealed class HeaderPageConfiguration<THeader> where THeader : new()
    {
        public PageConfiguration InnerPageMap { get; set; }
        public FixedSizeRecordDeclaration<THeader> Header { get; set; }

       
        private ushort HeaderSize => (ushort)Header.GetSize;
        internal HeaderedPage<THeader> CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize) 
        {
            return new HeaderedPage<THeader>(accessor,
                InnerPageMap.CreatePage(headers, accessor.GetChildAccessorWithStartShift(HeaderSize), reference, pageSize - HeaderSize), reference,this);
        }
    }
}
