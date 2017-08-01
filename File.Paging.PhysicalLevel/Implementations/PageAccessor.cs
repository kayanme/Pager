using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
 
    internal class PageAccessor: IPageAccessor
    {
        private IExtentAccessorFactory _disposer;
        private MemoryMappedViewAccessor _map;
        private int _startOffset;
        private int _pageSize;
        private uint _extentNumber;
        private int _pageType;
        internal PageAccessor(int startOffset,int pageSize,uint extentNumber,MemoryMappedViewAccessor accessor, IExtentAccessorFactory disposer)
        {
            _map = accessor;
            _startOffset = startOffset;
            _disposer = disposer;
            _pageSize = pageSize;
            _extentNumber = extentNumber;
        }
    
        public int PageSize => _pageSize;

        public uint ExtentNumber => _extentNumber;
   

        public void Flush()
        {
            if (!disposedValue)
               _map.Flush();
        }

        public byte[] GetByteArray(int position, int length)
        {
           if (disposedValue)
                throw new ObjectDisposedException("IPageAccessor");
            Debug.Assert(position + length <= _pageSize, "position + length <= _pageSize");
            var b = new byte[length];
            _map.ReadArray(position + _startOffset, b, 0, length);
            return b; 
        }

        public void SetByteArray(byte[] record, int position, int length)
        {
            if (disposedValue)
                throw new ObjectDisposedException("IPageAccessor");
            Debug.Assert(position + length <= _pageSize, "position + length <= _pageSize");
            _map.WriteArray(position + _startOffset, record, 0, length);
        }

        public IPageAccessor GetChildAccessorWithStartShift(ushort startShirt)
        {
            if (disposedValue)
                throw new ObjectDisposedException("IPageAccessor");
            return new PageAccessor(_startOffset + startShirt, _pageSize - startShirt, _extentNumber, _map, null);
        }

        private bool disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Flush();
                    _disposer?.ReturnAccessor(_map);
                }

                disposedValue = true;
            }
        }
        ~PageAccessor()
        {
            Dispose(true);
        }


        public  void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    
}
