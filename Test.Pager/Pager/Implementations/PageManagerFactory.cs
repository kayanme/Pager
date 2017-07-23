using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Implementations;

namespace Pager
{
    [Export(typeof(IPageManagerFactory))]
    public sealed class PageManagerFactory : IPageManagerFactory
    {
        public IPageManager CreateManager(string fileName, PageMapConfiguration configuration, bool createFileIfNotExists)
        {
            FileStream _file;          

            bool shouldInit = false;
            if (!File.Exists(fileName))
            {
                if (createFileIfNotExists)
                {
                    _file = File.Create(fileName);
                    shouldInit = true;
                }
                else
                    throw new FileNotFoundException($"Page file {fileName} not found");
            }
            else
                _file = File.Open(fileName, FileMode.Open);
            var children = new CompositionContainer(new AssemblyCatalog(typeof(PageManagerFactory).Assembly));
            var b = new CompositionBatch();
            b.AddExportedValue(configuration);
            b.AddExportedValue(_file);
            children.Compose(b);
                        
            children.Compose(new CompositionBatch());
            var a1 = children.GetExport<FileStream>();
            var a2 = children.GetExport<IUnderlyingFileOperator>();
            return children.GetExportedValue<IPageManager>();
        }
        private CompositionContainer _container;
        public PageManagerFactory()
        {
            _container = new CompositionContainer(new AssemblyCatalog(typeof(PageManagerFactory).Assembly));
        }
    }
}
