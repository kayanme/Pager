using File.Paging.PhysicalLevel.Classes.Configurations;

namespace File.Paging.PhysicalLevel.Contracts
{
    public interface IPageManagerFactory
    {
        IPageManager CreateManager(string fileName,PageManagerConfiguration configuration, bool createFileIfNotExists);
    }
}
