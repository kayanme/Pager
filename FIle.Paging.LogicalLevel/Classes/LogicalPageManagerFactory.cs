using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;
using FIle.Paging.LogicalLevel.Classes.Configurations;
using FIle.Paging.LogicalLevel.Classes.Factories;
using FIle.Paging.LogicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes
{
    public sealed class LogicalPageManagerFactory : ILogicalPageManagerFactory
    {
        public IPageManager CreateManager(string fileName, PageManagerConfiguration configuration, bool createFileIfNotExists)
        {
            var phys = _factory.CreateManager(fileName, configuration, createFileIfNotExists);
            var config = configuration as LogicalPageManagerConfiguration;
            return config != null ? new LogicalPageManager(phys, config,new LogicalPageFactory()) : phys;
        }

        readonly IPageManagerFactory _factory;
        public LogicalPageManagerFactory()
        {
            _factory = new PageManagerFactory();
        }
    }
}
