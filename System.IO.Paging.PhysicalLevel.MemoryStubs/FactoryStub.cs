using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;

namespace System.IO.Paging.PhysicalLevel.MemoryStubs
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
