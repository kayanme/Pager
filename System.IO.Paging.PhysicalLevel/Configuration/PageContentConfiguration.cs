namespace System.IO.Paging.PhysicalLevel.Configuration
{
    internal abstract class PageContentConfiguration
    {
        public ConsistencyConfiguration ConsistencyConfiguration;
     
        internal abstract Type RecordType { get; }

        internal ushort PageSize { get; set; }
    
        internal bool WithLogicalSort;

        internal abstract HeaderInfo ReturnHeaderInfo();

        public abstract void Verify();
    }
}
