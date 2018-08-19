using System.IO.Paging.PhysicalLevel.Configuration;

namespace System.IO.Paging.PhysicalLevel.Contracts
{
    public interface IPageManagerFactory
    {
        IPageManager CreateManager(string fileName,PageManagerConfiguration configuration, bool createFileIfNotExists);
    }
}
