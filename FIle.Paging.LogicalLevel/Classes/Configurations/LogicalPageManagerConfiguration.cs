using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes.Configurations;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    public class LogicalPageManagerConfiguration: PageManagerConfiguration
    {
        internal Dictionary<byte, LogicalPageConfiguration> Configuration = new Dictionary<byte, LogicalPageConfiguration>();

        public LogicalPageManagerConfiguration(PageSize pageSize) : base(pageSize)
        {
            
        }
    }
}
