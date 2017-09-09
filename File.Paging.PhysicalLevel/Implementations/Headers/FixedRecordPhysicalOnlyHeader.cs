using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private readonly FixedPageParametersCalculator _pageCalculator;

        //      private readonly int[] _pageAllocationMap;
        private readonly int _pamSize;
        private readonly int _lastMask;
        private int _usedRecords;

        public FixedRecordPhysicalOnlyHeader(IPageAccessor accessor, 
            FixedPageParametersCalculator pageCalculator,int initialUsed)
        {
            _fixedRecordSize = pageCalculator.FixedRecordSize;
            _accessor = accessor;
            _pageCalculator = pageCalculator;
            //  _pageAllocationMap = pageCalculator.PageAllocationMap;
            _pamSize = pageCalculator.PamSize;
            _lastMask = pageCalculator.LastMask;
            _usedRecords = initialUsed;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalcShiftFromPosition(int position) => _pamSize + position * _fixedRecordSize;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalcPositionFromShift(int shift) => (shift - _pamSize) / _fixedRecordSize;

        private int[] GetPam()
        {
            int[] pageAllocationMap = null;
            unsafe
            {
              
                _accessor.QueueByteArrayOperation(0, _pamSize,
                    b =>
                    {
                        pageAllocationMap = _pageCalculator.ProcessPam(b);
                    });
            }
            return pageAllocationMap;
        }

        public  IEnumerable<ushort> NonFreeRecords()
        {
            ushort curPosition = (ushort)_pamSize;
            var pageAllocationMap = GetPam();
            foreach (var p in pageAllocationMap)
            {
                unchecked
                {
                    if (p == (int) 0xFFFFFFFF)
                    {
                        for (int j = 0; j < 32; j++)
                        {                            
                            yield return curPosition;
                            curPosition += _fixedRecordSize;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < 32; j++)
                        {
                            var t = 1 << j;
                            if ((p & t) == t)
                            {                               
                                yield return curPosition;                             
                            }
                            curPosition += _fixedRecordSize;
                        }
                    }
                }
            }
        }

        private int _searchHint;

        private unsafe void MakePamOperation(ByteAction act)
        {
            _accessor.QueueByteArrayOperation(0,_pamSize,act);
        }

        
        public unsafe short TakeNewRecord(byte rType, ushort rSize)
        {
            Debug.Assert(rType == 0, "rType == 0");
            Debug.Assert(rSize == _fixedRecordSize, "rSize == _fixedRecordSize");
            var t = _searchHint;
            var pamLength = _pageCalculator.PamIntLength;
            short shift = -1;
            MakePamOperation(b =>
            {
                int* asInt = (int*)b;
                for (int ind = 0; ind < pamLength; ind++)
                {
                    var i = (t + ind) % pamLength;

                    var mask = *(asInt+i);
                    int targetMask;
                    unchecked
                    {
                        targetMask = i == pamLength - 1 ? ((int)(0x80_00_00_00) >> (_pageCalculator.BitsUnusedInLastInt - 1)) : 0;
                    }
                  
                    while ((mask | targetMask) != -1) //0xFFFFFFFF
                    {
                        var bits = new BitVector32(mask);
                        var bitsAvailableToset = (i == pamLength - 1 ? 32 - _pageCalculator.BitsUnusedInLastInt : 32);
                        for (var j = 0; j < bitsAvailableToset; j++)
                            if (!bits[1 << j])
                            {
                                bits[1 << j] = true;
                                if (Interlocked.CompareExchange(ref *(asInt + i), bits.Data, mask) == mask)
                                {
                                    _searchHint = i;
                                    shift = (short)CalcShiftFromPosition(i * 32 + j);
                                    Interlocked.Increment(ref _usedRecords);
                                    return;
                                }
                            }

                        mask = *(asInt + i);
                    }

                }
            });
            return shift;
        }    
     

        public unsafe bool IsRecordFree(ushort persistentRecordNum)
        {
            var pos = CalcPositionFromShift(persistentRecordNum);
            bool t = true;
            MakePamOperation(b =>
            {
                var i = (int*) b;
                var d = *(i + (pos / 32));
                var d2 = new BitVector32(d);
                t = !d2[1 << (pos % 32)];
            });
            return t;
        }

        public unsafe void FreeRecord(ushort persistentRecordNum)
        {
            var pos = CalcPositionFromShift(persistentRecordNum);
            int d;
            int change;
            MakePamOperation(b =>
            {
                var i = (int*)b;
                do
                {
                    d = *(i + (pos / 32));
                    var b2 = new BitVector32(d) {[1 << pos % 32] = false};
                    change = b2.Data;
                } while (Interlocked.CompareExchange(ref *(i + (pos / 32)), change, d) != d);
                Interlocked.Decrement(ref _usedRecords);
            });

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

      
        public void Compact()
        {
            throw new NotImplementedException();
        }

        public int TotalUsedSize => _usedRecords * _fixedRecordSize + _pamSize;
    }
}
