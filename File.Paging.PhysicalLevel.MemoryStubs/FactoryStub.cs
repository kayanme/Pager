using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;

namespace File.Paging.PhysicalLevel.MemoryStubs
{
    [Export(typeof(IPageManagerFactory))]
    public sealed class FactoryStub : IPageManagerFactory
    {
        public IPageManager CreateManager(string fileName, PageManagerConfiguration configuration, bool createFileIfNotExists)
        {
            return new PageManagerStub(configuration);
        }
    }
}
