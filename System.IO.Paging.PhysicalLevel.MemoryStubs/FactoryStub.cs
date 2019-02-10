using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;

namespace System.IO.Paging.PhysicalLevel.MemoryStubs
{
    [Export(typeof(IPageManagerFactory))]
    public sealed class FactoryStub : IPageManagerFactory
    {
      

        public IPageManager CreateManagerForExistingFile(string fileName, PageManagerConfiguration configuration)
        {
            return new PageManagerStub(configuration);
        }

        public IPageManager CreateManagerWithAutoFileCreation(string fileName, PageManagerConfiguration configuration)
        {
            return new PageManagerStub(configuration);
        }
    }
}
