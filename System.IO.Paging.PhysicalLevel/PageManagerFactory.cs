using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;

namespace System.IO.Paging.PhysicalLevel
{
    [Export(typeof(IPageManagerFactory))]
    public sealed class PageManagerFactory : IPageManagerFactory, IDisposable
    {
        private readonly AggregateCatalog _catalog;
        public IPageManager CreateManagerForExistingFile(string fileName, PageManagerConfiguration configuration)
        {
            FileStream file;
            if (!System.IO.File.Exists(fileName))
            {
                throw new FileNotFoundException($"Page file {fileName} not found");
            }
            else
                file = System.IO.File.Open(fileName, FileMode.Open);

            return CreateManager(configuration, file);
        }

        public IPageManager CreateManagerWithAutoFileCreation(string fileName, PageManagerConfiguration configuration)
        {
            FileStream file;
            if (!System.IO.File.Exists(fileName))
            {

                file = System.IO.File.Create(fileName);

            }
            else
                file = System.IO.File.Open(fileName, FileMode.Open);

            return CreateManager(configuration, file);
        }

        private IPageManager CreateManager(PageManagerConfiguration configuration, FileStream file)
        {
            var children = new CompositionContainer(_catalog);
            var b = new CompositionBatch();
            b.AddExportedValue(configuration);
            b.AddExportedValue(file);
            children.Compose(b);
            var a1 = children.GetExport<FileStream>();
            var a2 = children.GetExport<IUnderlyingFileOperator>();
            var a3 = children.GetExportedValue<IPageFactory>();
            return children.GetExportedValue<IPageManager>();
        }

        public PageManagerFactory()
        {
            _catalog =
                new AggregateCatalog(
                   new AssemblyCatalog(typeof(PageManagerFactory).Assembly),
                   new AssemblyCatalog(typeof(ILockManager<>).Assembly));
        }

        public void Dispose()
        {
            _catalog.Dispose();
        }
    }
}
