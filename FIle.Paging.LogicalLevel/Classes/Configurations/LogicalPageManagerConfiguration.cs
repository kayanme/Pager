using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    public class LogicalPageManagerConfiguration: PageManagerConfiguration
    {
        public Dictionary<byte, LogicalPageConfiguration> Configuration = new Dictionary<byte, LogicalPageConfiguration>();
        
    }
}
