using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;

namespace System.IO.Paging.PhysicalLevel.Classes
{
    internal class BufferedPage
    {
        public byte PageType;
        public IPageAccessor Accessor;
        public IPageAccessor ContentAccessor;
        public IPageHeaders Headers;
        public PageContentConfiguration Config;
        public PageHeadersConfiguration HeaderConfig;
        public bool MarkedForRemoval;
        public int UserCount;
    }
}
