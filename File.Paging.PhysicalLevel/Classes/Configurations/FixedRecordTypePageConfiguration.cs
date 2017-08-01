using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Contracts;
using Pager.Implementations;

namespace Pager.Classes
{
    public class FixedRecordTypePageConfiguration<TRecordType> : PageConfiguration where TRecordType : TypedRecord, new()
    {
        internal override Type RecordType => typeof(TRecordType);
        public FixedSizeRecordDeclaration<TRecordType> RecordMap { get; set; }

        internal override IPageHeaders CreateHeaders(IPageAccessor accessor,ushort shift)
        {
            return new FixedRecordPageHeaders(accessor.GetChildAccessorWithStartShift(shift), (ushort)RecordMap.GetSize);
        }

        internal override IPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize)
        {
            return new FixedRecordTypedPage<TRecordType>(headers, accessor, reference, pageSize, this);
        }
    }
}
