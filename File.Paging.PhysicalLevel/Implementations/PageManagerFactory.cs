using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IPageManagerFactory))]
    public sealed class PageManagerFactory : IPageManagerFactory,IDisposable
    {
        private readonly AssemblyCatalog _catalog;
        public IPageManager CreateManager(string fileName, PageManagerConfiguration configuration, bool createFileIfNotExists)
        {
            FileStream file;          

            
            if (!System.IO.File.Exists(fileName))
            {
                if (createFileIfNotExists)
                {
                    file = System.IO.File.Create(fileName);                   
                }
                else
                    throw new FileNotFoundException($"Page file {fileName} not found");
            }
            else
                file = System.IO.File.Open(fileName, FileMode.Open);
            var children = new CompositionContainer(_catalog);
            var b = new CompositionBatch();
            b.AddExportedValue(configuration);
            b.AddExportedValue(file);
            children.Compose(b);                                 
            var a1 = children.GetExport<FileStream>();
            var a2 = children.GetExport<IUnderlyingFileOperator>();
            return children.GetExportedValue<IPageManager>();
        }
        
        public PageManagerFactory()
        {
            _catalog = new AssemblyCatalog(typeof(PageManagerFactory).Assembly);
        }

        public void Dispose()
        {
            _catalog.Dispose();
        }
    }
}
