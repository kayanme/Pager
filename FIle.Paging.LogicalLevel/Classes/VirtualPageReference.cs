using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace FIle.Paging.LogicalLevel.Classes
{
    internal sealed class VirtualPageReference:PageReference
    {
        public byte PageType { get; }
        public VirtualPageReference(int pageNum,byte pageType) : base(pageNum)
        {
            PageType = pageType;
        }
    }
}
