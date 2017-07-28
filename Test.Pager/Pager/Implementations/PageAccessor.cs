using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

        public void Dispose()
        {            
            _disposer?.ReturnAccessor(_map);
        }

        public void Flush()
        {
            _map.Flush();
        }

        public byte[] GetByteArray(int position, int length)
        {
            var b = new byte[length];
            _map.ReadArray(position + _startOffset, b, 0, length);
            return b;
        }

        public void SetByteArray(byte[] record, int position, int length)
        {
            _map.WriteArray(position + _startOffset, record, 0, length);
        }

        public IPageAccessor GetChildAccessorWithStartShift(ushort startShirt)
        {
            return new PageAccessor(_startOffset + startShirt, _pageSize - startShirt, _extentNumber, _map, null);
        }
    }

    
}
