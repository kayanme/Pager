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
      

        public IPageManager CreateManagerWithAutoFileCreation(string fileName, PageManagerConfiguration configuration)
        {
            var phys = _factory.CreateManagerWithAutoFileCreation(fileName, configuration);
            var config = configuration as LogicalPageManagerConfiguration;
            return config != null ? new LogicalPageManager(phys, config, new LogicalPageFactory()) : phys;
        }

        public IPageManager CreateManagerForExistingFile(string fileName, PageManagerConfiguration configuration)
        {
            var phys = _factory.CreateManagerForExistingFile(fileName, configuration);
            var config = configuration as LogicalPageManagerConfiguration;
            return config != null ? new LogicalPageManager(phys, config, new LogicalPageFactory()) : phys;

        }

        readonly IPageManagerFactory _factory;
        public LogicalPageManagerFactory()
        {
            _factory = new PageManagerFactory();
        }
    }
}
