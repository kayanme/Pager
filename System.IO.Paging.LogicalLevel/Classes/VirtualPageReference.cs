using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.LogicalLevel.Classes
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
