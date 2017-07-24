using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager.Implementations
{
    [Export(typeof(IExtentAccessorFactory))]
    internal class ExtentAccessorFactory : IExtentAccessorFactory
    {
        IUnderlyingFileOperator _file;

        private ConcurrentDictionary<MemoryMappedViewAccessor, MemoryMappedFile> _accessorsLent = new ConcurrentDictionary<MemoryMappedViewAccessor, MemoryMappedFile>();

        [ImportingConstructor]
        internal ExtentAccessorFactory(IUnderlyingFileOperator file)
        {
            _file = file;
        }

        public IPageAccessor GetAccessor(long pageOffset, int pageLength)
        {
            var extentNumber = pageOffset / Extent.Size;
            var extentBorder = extentNumber * Extent.Size;
            var map = _file.GetMappedFile(pageOffset + pageLength+Extent.Size);
         
            var accessor = map.CreateViewAccessor(extentBorder, Extent.Size);
            _accessorsLent.TryAdd(accessor, map);
            return new PageAccessor((int)(pageOffset - extentBorder),pageLength,(uint)extentNumber,accessor, this);
        }

        public void ReturnAccessor(MemoryMappedViewAccessor map)
        {
            map.Dispose();
            MemoryMappedFile f;
            _accessorsLent.TryRemove(map, out f);
            _file.ReturnMappedFile(f);
        }

        
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var t in _accessorsLent.Values)
                    {
                        t.Dispose();
                    }
                    GC.SuppressFinalize(this);
                }

                disposedValue = true;
            }
        }


        ~ExtentAccessorFactory()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}
