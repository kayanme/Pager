using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    internal abstract class BindedToPhysicalPageConfiguration:LogicalPageConfiguration
    {
        public abstract IPage CreateLogicalPage(IPage physicalPage);

    }
}
