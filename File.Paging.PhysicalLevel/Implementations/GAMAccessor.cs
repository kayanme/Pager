using System;
using System.ComponentModel.Composition;
using System.IO.MemoryMappedFiles;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IGamAccessor))]
    internal class GamAccessor:IGamAccessor
    {
        private readonly IUnderlyingFileOperator _fileOperator;
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly MemoryMappedFile _mapToReturn;
        [ImportingConstructor]
        internal GamAccessor(IUnderlyingFileOperator fileOperator)
        {
            _fileOperator = fileOperator;
            _mapToReturn = _fileOperator.GetMappedFile(Extent.Size);
            _accessor = _mapToReturn.CreateViewAccessor(0, Extent.Size);
        }

        public void InitializeGam()
        {                         
            
        }

        public void MarkPageFree(int pageNum)
        {
            lock (_accessor)
            {
                _accessor.Write(pageNum, 0);
                _accessor.Flush();
            }
        }
        public byte GetPageType(int pageNum)
        {
            return _accessor.ReadByte(pageNum);
        }

        public void SetPageType(int pageNum, byte pageType)
        {
             _accessor.Write(pageNum,pageType);
        }

        public int MarkPageUsed(byte pageType)
        {
            lock (_accessor)
            {
                for (int i = 0; i < Extent.Size; i++)
                {
                   var pageMark = _accessor.ReadByte(i);
                    if (pageMark == 0)
                    {
                        _accessor.Write(i, pageType);
                        return i;
                    }
                }
                throw new InvalidOperationException("File reaches it's maximum size");
            }
        }

        private bool _disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _fileOperator.ReturnMappedFile(_mapToReturn);
                    _accessor.Dispose();
                }

                _disposedValue = true;
            }
        }
        ~GamAccessor()
        {
            Dispose(true);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


       
    }
}
