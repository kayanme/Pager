using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Configuration;

namespace System.IO.Paging.LogicalLevel.Configuration
{
    public class LogicalPageManagerConfiguration: PageManagerConfiguration
    {
        internal Dictionary<byte, LogicalPageConfiguration> Configuration = new Dictionary<byte, LogicalPageConfiguration>();

        public LogicalPageManagerConfiguration(PageSize pageSize) : base(pageSize)
        {
            
        }
    }
}
