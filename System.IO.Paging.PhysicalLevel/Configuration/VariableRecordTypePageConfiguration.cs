namespace System.IO.Paging.PhysicalLevel.Configuration
{
    internal sealed class VariableRecordTypePageConfiguration<TRecord> : PageContentConfiguration where TRecord:struct
    {
        internal override Type RecordType => typeof(TRecord);
      
      
        public VariableSizeRecordDeclaration<TRecord> RecordMap;
      

        public VariableRecordTypePageConfiguration()
        {        
            ConsistencyConfiguration = new ConsistencyConfiguration {ConsistencyAbilities = ConsistencyAbilities.None};

        }


        internal override HeaderInfo ReturnHeaderInfo()
        {
            return new HeaderInfo(false,WithLogicalSort,0);
        }

        public override void Verify()
        {
           
        }
    }

}
