using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.IO.MemoryMappedFiles;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IExtentAccessorFactory))]
    internal class ExtentAccessorFactory : IExtentAccessorFactory
    {
        readonly IUnderlyingFileOperator _file;

        private readonly ConcurrentDictionary<MemoryMappedViewAccessor, MemoryMappedFile> _accessorsLent = new ConcurrentDictionary<MemoryMappedViewAccessor, MemoryMappedFile>();

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
            _accessorsLent.TryRemove(map, out MemoryMappedFile f);
            _file.ReturnMappedFile(f);
        }

        
        private bool _disposedValue = false; 
        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach(var t in _accessorsLent.Values)
                    {
                        t.Dispose();
                    }              
                }

                _disposedValue = true;
            }
        }
        ~ExtentAccessorFactory()
        {
            Dispose(false);
        }

       
        public void Dispose()
        {          
            Dispose(true);           
            GC.SuppressFinalize(this);
        }
    }
}
