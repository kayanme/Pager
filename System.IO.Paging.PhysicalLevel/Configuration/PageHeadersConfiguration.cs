namespace System.IO.Paging.PhysicalLevel.Configuration
{
    internal abstract class PageHeadersConfiguration
    {
        public PageContentConfiguration InnerPageMap { get; set; }
      
        public abstract ushort HeaderSize { get; }
    }

    internal sealed class PageHeadersConfiguration<THeader> : PageHeadersConfiguration where THeader : new() 
    {

        public FixedSizeRecordDeclaration<THeader> Header { get; set; }

        public override ushort HeaderSize => (ushort)Header.GetSize;        

      
    }
}