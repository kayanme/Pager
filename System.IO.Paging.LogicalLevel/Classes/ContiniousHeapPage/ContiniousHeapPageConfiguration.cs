using System.IO.Paging.LogicalLevel.Configuration;

namespace System.IO.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
    internal sealed class ContiniousHeapPageConfiguration<TRecord> : VirtualPageConfiguration
        where TRecord : struct 
    {
        public byte HeaderPageType;

      
    }
}

