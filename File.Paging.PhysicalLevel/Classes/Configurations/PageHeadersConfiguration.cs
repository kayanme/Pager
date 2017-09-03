using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Configurations
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