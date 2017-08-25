using System;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using File.Paging.PhysicalLevel.Implementations.Headers;

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
            var headerAccessor = accessor.GetChildAccessorWithStartShift(shift);
            if (!WithLogicalSort)
            {
                var pageCalculator =
                    new FixedPageParametersCalculator((ushort) headerAccessor.PageSize, (ushort) RecordMap.GetSize);
                pageCalculator.CalculatePageParameters();
                var rawPam = headerAccessor.GetByteArray(0, pageCalculator.PamSize);
                pageCalculator.ProcessPam(rawPam);
                return new FixedRecordPhysicalOnlyHeader(headerAccessor, pageCalculator);
            }
            else
            {
                var pageCalculator =
                    new FixedPageParametersCalculator((ushort)headerAccessor.PageSize, (ushort)RecordMap.GetSize,16);
                pageCalculator.CalculatePageParameters();
                var rawPam = headerAccessor.GetByteArray(0, pageCalculator.PamSize);
                pageCalculator.ProcessPam(rawPam);
                return new FixedRecordWithLogicalOrderHeader(headerAccessor, pageCalculator);
            }
          
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
