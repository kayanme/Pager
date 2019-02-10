using System.IO.Paging.PhysicalLevel.Configuration;

namespace System.IO.Paging.PhysicalLevel.Contracts
{
    public interface IPageManagerFactory
    {
        IPageManager CreateManagerWithAutoFileCreation(string fileName,PageManagerConfiguration configuration);
        IPageManager CreateManagerForExistingFile(string fileName, PageManagerConfiguration configuration);
    }
}
