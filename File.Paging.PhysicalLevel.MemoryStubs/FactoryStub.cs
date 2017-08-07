using System.ComponentModel.Composition;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

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
