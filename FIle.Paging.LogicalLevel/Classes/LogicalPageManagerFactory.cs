using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIle.Paging.LogicalLevel.Classes.Configurations;
using FIle.Paging.LogicalLevel.Contracts;
using Pager;

namespace FIle.Paging.LogicalLevel.Classes
{
    public sealed class LogicalPageManagerFactory : ILogicalPageManagerFactory
    {
        public IPageManager CreateManager(string fileName, Pager.PageManagerConfiguration configuration, bool createFileIfNotExists)
        {
            var phys = _factory.CreateManager(fileName, configuration, createFileIfNotExists);
            if (configuration is Configurations.LogicalPageManagerConfiguration)
            {
                return new LogicalPageManager((IPageManager)phys, (Configurations.LogicalPageManagerConfiguration)(configuration as Configurations.LogicalPageManagerConfiguration));
            }
            else
                return phys;
        }

        IPageManagerFactory _factory;
        public LogicalPageManagerFactory()
        {
            _factory = new PageManagerFactory();
        }
    }
}
