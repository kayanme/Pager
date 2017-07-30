using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    [Export(typeof(IGAMAccessor))]
    internal class GAMAccessor:IGAMAccessor
    {
        private IUnderlyingFileOperator _fileOperator;
        private MemoryMappedViewAccessor _accessor;
        private MemoryMappedFile _mapToReturn;
        [ImportingConstructor]
        internal GAMAccessor(IUnderlyingFileOperator fileOperator)
        {
            _fileOperator = fileOperator;
            _mapToReturn = _fileOperator.GetMappedFile(Extent.Size);
            _accessor = _mapToReturn.CreateViewAccessor(0, Extent.Size);
        }

        public void InitializeGAM()
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

        private bool disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileOperator.ReturnMappedFile(_mapToReturn);
                    _accessor.Dispose();
                }

                disposedValue = true;
            }
        }
        ~GAMAccessor()
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
