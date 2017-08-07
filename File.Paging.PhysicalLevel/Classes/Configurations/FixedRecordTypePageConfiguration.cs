using System;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal sealed class FixedRecordTypePageConfiguration<TRecordType> : PageContentConfiguration where TRecordType : TypedRecord, new()
    {
        internal override Type RecordType => typeof(TRecordType);
        internal FixedSizeRecordDeclaration<TRecordType> RecordMap { get; set; }

        public FixedRecordTypePageConfiguration()
        {
            ConsistencyConfiguration = new ConsistencyConfiguration{ConsistencyAbilities = ConsistencyAbilities.None};
        }

        internal override IPageHeaders CreateHeaders(IPageAccessor accessor,ushort shift)
        {
            return new FixedRecordPageHeaders(accessor.GetChildAccessorWithStartShift(shift), (ushort)RecordMap.GetSize);
        }

        public override void Verify()
        {
            
        }

        internal override IPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize,byte pageType)
        {
            return new FixedRecordTypedPage<TRecordType>(headers, accessor, reference, pageSize, this, pageType);
        }
    }
}
