namespace System.IO.Paging.PhysicalLevel.Configuration
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
