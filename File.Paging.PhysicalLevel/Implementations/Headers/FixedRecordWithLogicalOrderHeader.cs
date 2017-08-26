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
    internal sealed class FixedRecordWithLogicalOrderHeader : IPageHeaders
    {
        private readonly ushort _fixedRecordSize;
        private readonly IPageAccessor _accessor;
        private readonly int[] _pageAllocationMap;
        private int _pamSize;
      
        private int _usedRecords;

      

        public FixedRecordWithLogicalOrderHeader(IPageAccessor accessor, FixedPageParametersCalculator pageCalculator)
        {
            _fixedRecordSize = pageCalculator.FixedRecordSize;
            _accessor = accessor;
            _pageAllocationMap = pageCalculator.PageAllocationMap;
            _pamSize = pageCalculator.PamSize;
         
            _usedRecords = pageCalculator.UsedRecords;
        }



        private int CalcShiftFromPosition(int position) => _pamSize + position * _fixedRecordSize;

        private int CalcPositionFromShift(int shift) => (shift - _pamSize) / _fixedRecordSize;

        public IEnumerable<ushort> NonFreeRecords()
        {
            
            var orders = new ushort[_usedRecords];
          ;
            var lastLookedRecord = 0;
            for (var currentRecord = 0; currentRecord <orders.Length;currentRecord++)
            {
                if (orders[currentRecord] == 0)
                for (; lastLookedRecord <= _usedRecords/2; lastLookedRecord++)
                {
                    var orderNum = FixUshort((ushort)(_pageAllocationMap[lastLookedRecord] >> 16)) - 1;//порядок начинается с 1
                    if (orderNum >orders.Length)
                        yield return (ushort) CalcShiftFromPosition(lastLookedRecord * 2+1);
                    else if (orderNum>=0 && orderNum < orders.Length)//кто-то может присвоить плохой номер во время перебора
                    {
                        orders[orderNum] = (ushort) CalcShiftFromPosition(lastLookedRecord * 2+1);
                    }
                    orderNum = FixUshort((ushort)(_pageAllocationMap[lastLookedRecord]& 0xFFFF)) - 1;
                    if (orderNum > orders.Length)
                        yield return (ushort)CalcShiftFromPosition(lastLookedRecord * 2);
                    else if (orderNum >= 0 && orderNum < _usedRecords) 
                    {
                        orders[orderNum] = (ushort) CalcShiftFromPosition(lastLookedRecord * 2);
                    }
                    if (orderNum == currentRecord || orderNum - 1 == currentRecord)
                    {
                        lastLookedRecord++;
                        break;
                    }
                }
                if(lastLookedRecord == _usedRecords)                
                    yield break;
              // ;
              if (orders[currentRecord] !=0)
                yield return (ushort)(orders[currentRecord]);
              
            }            
        }

        public short TakeNewRecord(byte rType, ushort rSize)
        {
            Debug.Assert(rType == 0, "rType == 0");
            Debug.Assert(rSize == _fixedRecordSize, "rSize == _fixedRecordSize");
            var recordTaken = -1;
            for (int i = 0; i < _pageAllocationMap.Length; i++)
            {
                var oldMask = _pageAllocationMap[i];
                int newMask;
               
                if ((oldMask & 0xFFFF) == 0)
                {

                    newMask = oldMask | 0xFFFF;
                    if (Interlocked.CompareExchange(ref _pageAllocationMap[i], newMask, oldMask) == oldMask)
                    {
                        recordTaken = i*2;
                        break;
                    }
                }
                if ((i != _pageAllocationMap.Length - 1 || _pamSize % 4 == 0) && ((uint)oldMask & 0xFFFF0000) == 0)
                {
                    unchecked
                    {
                        newMask = oldMask | (int)0xFFFF0000;
                    }
                    if (Interlocked.CompareExchange(ref _pageAllocationMap[i], newMask, oldMask) == oldMask)
                    {
                        recordTaken = i*2+1;
                        break;
                    }
                }
            }
            if (recordTaken == -1)
              return -1;
            Interlocked.Increment(ref _usedRecords);
            SyncPam();
            return (short)CalcShiftFromPosition(recordTaken);
        }

        private volatile int _reordering  =0;
        private volatile int _syncer = 0;

        private unsafe void SyncPam()
        {
          
            try
            {
                while (Interlocked.CompareExchange(ref _syncer, 1, 0) != 0) ;
                var bytes = new byte[_pamSize];
                fixed (void* src  = _pageAllocationMap)
                fixed (void* dst = bytes)
                {
                    Buffer.MemoryCopy(src,dst,_pamSize,_pamSize);
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
            var d = _pageAllocationMap[pos/2];
            if (pos % 2 == 1)
            {
                return (uint)d >> 16 == 0;
            }
            else
            {
                return (d & 0xFFFF) == 0;
            }
        }

        private static ushort FixUshort(ushort num) =>(ushort)( num >> 8 | num << 8);

        public void FreeRecord(ushort persistentRecordNum)
        {
            var pos = CalcPositionFromShift(persistentRecordNum);
            int d;
            int change;
            do
            {
                d = _pageAllocationMap[pos / 2];
                unchecked
                {
                    if (pos % 2 == 1)
                    {
                        change = 0xFFFF & d;
                    }
                    else
                    {
                        change = (int)0xFFFF0000 & d;
                    }
                }
                if (change == d)
                    return;
            } while (Interlocked.CompareExchange(ref _pageAllocationMap[pos / 2], change, d) != d);
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
            try
            {
                while (Interlocked.CompareExchange(ref _reordering, 1, 0) != 0) ;
                for (ushort i = 1; i <= recordsInOrder.Length; i++)
                {
                    var p = CalcPositionFromShift(recordsInOrder[i-1]);
                    int oldMap, newMap;
                                                       
                    do
                    {
                        oldMap = _pageAllocationMap[p / 2];
                        unchecked
                        {

                            if (p % 2 == 1)
                            {
                                newMap = oldMap & 0xFFFF | (FixUshort(i)) << 16;
                            }
                            else
                            {
                                newMap = oldMap & (int) 0xFFFF0000 | (FixUshort(i));
                            }
                        }
                    } while (Interlocked.CompareExchange(ref _pageAllocationMap[p/2],newMap,oldMap)!=oldMap);                    
                }
                
            }
            finally
            {
                _reordering = 0;
            }
            SyncPam();
        }

        public void DropOrder(ushort persistentRecordNum)
        {
            var p = CalcPositionFromShift(persistentRecordNum);
            int oldMap, newMap;
            do
            {
                oldMap = _pageAllocationMap[p / 2];
                unchecked
                {

                    if (p % 2 == 0)
                    {
                        newMap = oldMap | (int)0xFFFF0000;
                    }
                    else
                    {
                        newMap = oldMap & 0xFFFF;
                    }
                }
            } while (Interlocked.CompareExchange(ref _pageAllocationMap[p / 2], newMap, oldMap) != oldMap);
        }


        public void Compact()
        {
         
        }

        public int TotalUsedSize => _usedRecords * _fixedRecordSize + _pamSize;
    }
}
