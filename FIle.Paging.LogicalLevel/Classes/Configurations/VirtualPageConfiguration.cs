using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    internal abstract class VirtualPageConfiguration : LogicalPageConfiguration
    {
        public byte PageTypeNum;
        public abstract IPage CreateLogicalPage(IPageManager physicalPageManager);

    }
}