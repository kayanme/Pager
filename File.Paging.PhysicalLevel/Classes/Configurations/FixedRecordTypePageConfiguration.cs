using System;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using File.Paging.PhysicalLevel.Implementations.Headers;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal sealed class FixedRecordTypePageConfiguration<TRecordType> : PageContentConfiguration where TRecordType :  new()
    {
        internal override Type RecordType => typeof(TRecordType);
        internal FixedSizeRecordDeclaration<TRecordType> RecordMap { get; set; }
     
        public FixedRecordTypePageConfiguration()
        {
            ConsistencyConfiguration = new ConsistencyConfiguration{ConsistencyAbilities = ConsistencyAbilities.None};
        }

        internal override HeaderInfo ReturnHeaderInfo()
        {
            return new HeaderInfo(true,WithLogicalSort,(ushort)RecordMap.GetSize);
        }

        public override void Verify()
        {
            
        }

      
    }
}
