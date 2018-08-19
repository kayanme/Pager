using System.IO.Paging.LogicalLevel.Classes;
using System.IO.Paging.LogicalLevel.Classes.Factories;
using System.IO.Paging.LogicalLevel.Configuration;
using System.IO.Paging.LogicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;

namespace System.IO.Paging.LogicalLevel
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
