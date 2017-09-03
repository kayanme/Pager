using System;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Configurations
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
