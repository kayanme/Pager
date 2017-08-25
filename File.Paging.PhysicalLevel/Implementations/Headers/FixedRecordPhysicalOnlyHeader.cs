using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Implementations.Headers
{
    internal sealed class FixedRecordPhysicalOnlyHeader : IPageHeaders
    {
        private readonly ushort _fixedRecordSize;
        private readonly IPageAccessor _accessor;
        private int[] _pageAllocationMap;
        private int _pamSize;
        private int _lastMask;
        private int _usedRecords;

        public FixedRecordPhysicalOnlyHeader(IPageAccessor accessor, FixedPageParametersCalculator pageCalculator)
        {
            _fixedRecordSize = pageCalculator.FixedRecordSize;
            _accessor = accessor;
            _pageAllocationMap = pageCalculator.PageAllocationMap;
            _pamSize = pageCalculator.PamSize;
            _lastMask = pageCalculator.LastMask;
            _usedRecords = pageCalculator.UsedRecords;
        }



        private int CalcShiftFromPosition(int position) => _pamSize + position * _fixedRecordSize;

        private int CalcPositionFromShift(int shift) => (shift - _pamSize) / _fixedRecordSize;

        public IEnumerable<ushort> NonFreeRecords()
        {
            for (int i = 0; i < _pageAllocationMap.Length; i++)
            {
                var bits = new BitVector32(_pageAllocationMap[i]);

                for (var j = 0; j <32; j++)
                    if (bits[1 << j])
                        yield return (ushort) (CalcShiftFromPosition(i * 32 + j));
            }
        }

        public short TakeNewRecord(byte rType, ushort rSize)
        {
            Debug.Assert(rType == 0, "rType == 0");
            Debug.Assert(rSize == _fixedRecordSize, "rSize == _fixedRecordSize");
            for (int i = 0; i < _pageAllocationMap.Length; i++)
            {
                var mask = _pageAllocationMap[i] | (i == _pageAllocationMap.Length -1? _lastMask : 0);
                while (mask != -1) //0xFFFFFFFF
                {
                    var bits = new BitVector32(mask);
                    for (var j = 0; j <32; j++)
                        if (!bits[1 << j])
                        {
                            bits[1 << j] = true;
                            if (Interlocked.CompareExchange(ref _pageAllocationMap[i], bits.Data, mask) == mask)
                            {
                                var shift = CalcShiftFromPosition(i * 32 + j);
                                Interlocked.Increment(ref _usedRecords);
                                SyncPam();
                                return (short) shift;
                            }
                        }

                    mask = _pageAllocationMap[i] | (i == _pageAllocationMap.Length ? _lastMask : 0);
                }
            }
          
            return -1;
        }

        private volatile int _syncer = 0;

        private unsafe void SyncPam()
        {
            try
            {
                while (Interlocked.CompareExchange(ref _syncer, 1, 0) != 0) ;
                var bytes = new byte[_pamSize];
                fixed (void* src = _pageAllocationMap)
                fixed (void* dst = bytes)
                {
                    Buffer.MemoryCopy(src, dst, _pamSize, _pamSize);
                }
                _accessor.SetByteArray(bytes, 0, _pamSize);
            }
            finally
            {
                _syncer = 0;
            }
        }
    

        public bool IsRecordFree(ushort persistentRecordNum)
        {
            var pos = CalcPositionFromShift(persistentRecordNum);
            var d = _pageAllocationMap[pos / 32];
            var d2 = new BitVector32(d);
            return !d2[1<<(pos % 32)];
        }

        public void FreeRecord(ushort persistentRecordNum)
        {
            var pos = CalcPositionFromShift(persistentRecordNum);
            int d;
            int change;
            do
            {
                d = _pageAllocationMap[pos / 32];
                var b = new BitVector32(d) {[1<<pos % 32] = false};
                change = b.Data;
            } while (Interlocked.CompareExchange(ref _pageAllocationMap[pos / 32], change, d) != d);
            Interlocked.Decrement(ref _usedRecords);
           
            SyncPam();
        }

        public ushort RecordCount => (ushort)_usedRecords;
        public ushort RecordShift(ushort persistentRecordNum) => persistentRecordNum;

        public byte RecordType(ushort persistentRecordNum) => 0;

        public ushort RecordSize(ushort persistentRecordNum) => _fixedRecordSize;

        public void SetNewRecordInfo(ushort persistentRecordNum, ushort rSize, byte rType)
        {
            Debug.Assert(rType == 0, "rType == 0");
            Debug.Assert(rSize == _fixedRecordSize, "rSize == _fixedRecordSize");
        }

        public void ApplyOrder(ushort[] recordsInOrder)
        {
            throw new NotImplementedException();
        }

        public void DropOrder(ushort persistentRecordNum)
        {
            throw new NotImplementedException();
        }

        public void SwapRecords(ushort recordOne, ushort recordTwo)
        {
            throw new NotImplementedException();
        }

        public void Compact()
        {
            throw new NotImplementedException();
        }

        public int TotalUsedSize => _usedRecords * _fixedRecordSize + _pamSize;
    }
}
