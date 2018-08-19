using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Linq;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IGamAccessor))]
    internal class GamAccessor:IGamAccessor
    {
        private readonly IUnderlyingFileOperator _fileOperator;
    
        private MemoryMappedFile _mapToReturn;
        private ushort _pageSize;
        private ushort _gamPagesCount;
        private int _bestCandidate;
        private List<MemoryMappedViewAccessor> _accessors;

        [ImportingConstructor]
        internal GamAccessor(IUnderlyingFileOperator fileOperator)
        {
          
            _fileOperator = fileOperator;
            _bestCandidate = 0;
        }

        private void CreateAccessors()
        {
            _mapToReturn = _fileOperator.GetMappedFile(_pageSize * Extent.Size * (_gamPagesCount - 1) + Extent.Size * _gamPagesCount);
            _accessors = Enumerable.Range(0, _gamPagesCount).Select(k => _mapToReturn.CreateViewAccessor((_pageSize + 1) * Extent.Size * k, Extent.Size,MemoryMappedFileAccess.ReadWrite)).ToList();
        }

        public void InitializeGam(ushort pageSize)
        {
            _pageSize = pageSize;
            _gamPagesCount = (ushort)(_fileOperator.FileSize / ((_pageSize + 1) * Extent.Size+1)+1);
            CreateAccessors();
        }
        private int accNum(int pageNum)=> pageNum / Extent.Size;
        public void MarkPageFree(int pageNum)
        {
            lock (_accessors)
            {
                var an = accNum(pageNum );
                _accessors[an].Write(pageNum % Extent.Size, 0);
                _accessors[an].Flush();
            }
        }
        public byte GetPageType(int pageNum)
        {
            var an = accNum(pageNum);
            return _accessors[an].ReadByte(pageNum % Extent.Size);
        }

        public void SetPageType(int pageNum, byte pageType)
        {
            var an = accNum(pageNum);
            _accessors[an].Write(pageNum % Extent.Size, pageType);
        }
        private int? CheckBestCandidate(byte pageType)
        {
            if (_bestCandidate >= _gamPagesCount * Extent.Size)
            {
                return CreateNewGam(pageType);
            }
            var acc = _bestCandidate / Extent.Size;
            var shift = _bestCandidate % Extent.Size;
            if (_accessors[acc].ReadByte(shift) == 0)
            {
                _accessors[acc].Write(shift, pageType);
                _bestCandidate = shift + acc * Extent.Size+1;
                return shift + acc * Extent.Size;
            }
            return null;
        }

        private int CreateNewGam(int pageType)
        {
            _gamPagesCount++;
            _fileOperator.ReturnMappedFile(_mapToReturn);
            foreach (var acc in _accessors)
                acc.Dispose();
            CreateAccessors();
            _accessors.Last().Write(0, pageType);
            _bestCandidate++;
            return Extent.Size * (_gamPagesCount - 1);
        }

        public int MarkPageUsed(byte pageType)
        {
            lock (_accessors)
            {
                var page = CheckBestCandidate(pageType);
                if (page.HasValue)
                    return page.Value;
              for (var k = 0;k<_gamPagesCount;k++)
                for (int i = 0; i < Extent.Size; i++)
                {
                    var pageMark = _accessors[k].ReadByte(i);
                    if (pageMark == 0)
                    {
                        _accessors[k].Write(i, pageType);
                        _bestCandidate = i + k * Extent.Size+1;
                        return i + k*Extent.Size;
                    }
                }
                return CreateNewGam(pageType);
                
            }
        }

        private bool _disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Debug.Assert(_mapToReturn != null, "_mapToReturn != null");
                    _fileOperator.ReturnMappedFile(_mapToReturn);
                    foreach(var acc in _accessors)
                       acc.Dispose();
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

        public long GamShift(int pageNum)
        {
            var gamCount = pageNum / Extent.Size + 1;
            return gamCount * Extent.Size;
        }
    }
}
