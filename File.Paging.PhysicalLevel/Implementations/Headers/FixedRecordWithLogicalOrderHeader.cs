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
        private readonly FixedPageParametersCalculator _pageCalculator;


        private int _usedRecords;

      

        public FixedRecordWithLogicalOrderHeader(IPageAccessor accessor, FixedPageParametersCalculator pageCalculator)
        {
            _fixedRecordSize = pageCalculator.FixedRecordSize;
            _accessor = accessor;
            _pageCalculator = pageCalculator;


            _usedRecords = pageCalculator.UsedRecords;
        }



        private int CalcShiftFromPosition(int position) => _pageCalculator.PamSize + position * _fixedRecordSize;

        private int CalcPositionFromShift(int shift) => (shift - _pageCalculator.PamSize) / _fixedRecordSize;


        private int[] GetPam()
        {
            int[] pageAllocationMap = null;
            unsafe
            {

                _accessor.QueueByteArrayOperation(0, _pageCalculator.PamSize,
                    b =>
                    {
                        pageAllocationMap = _pageCalculator.ProcessPam(b);
                    });
            }
            return pageAllocationMap;
        }

        public IEnumerable<ushort> NonFreeRecords()
        {
            
            var orders = new ushort[_usedRecords];
            var pageAllocationMap = GetPam();
            var lastLookedRecord = 0;
            for (var currentRecord = 0; currentRecord <orders.Length;currentRecord++)
            {
                if (orders[currentRecord] == 0)
                for (; lastLookedRecord <=(uint)(orders.Length-1)>>1; lastLookedRecord++)
                {
                    var orderNum = FixUshort((ushort)(pageAllocationMap[lastLookedRecord] >> 16)) - 1;//порядок начинается с 1
                    if (orderNum >orders.Length)
                        yield return (ushort) CalcShiftFromPosition(lastLookedRecord * 2+1);
                    else if (orderNum>=0 && orderNum < orders.Length)//кто-то может присвоить плохой номер во время перебора
                    {
                        orders[orderNum] = (ushort) CalcShiftFromPosition(lastLookedRecord * 2+1);
                    }
                    orderNum = FixUshort((ushort)(pageAllocationMap[lastLookedRecord]& 0xFFFF)) - 1;
                    if (orderNum > orders.Length)
                        yield return (ushort)CalcShiftFromPosition(lastLookedRecord * 2);
                    else if (orderNum >= 0 && orderNum < orders.Length) 
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
              
              if (orders[currentRecord] !=0)
                yield return (ushort)(orders[currentRecord]);
              
            }            
        }

        public unsafe short TakeNewRecord(byte rType, ushort rSize)
        {
            Debug.Assert(rType == 0, "rType == 0");
            Debug.Assert(rSize == _fixedRecordSize, "rSize == _fixedRecordSize");
            var recordTaken = -1;
            _accessor.QueueByteArrayOperation(0, _pageCalculator.PamSize,
                b =>
                {
                    var asInt = (int*) b;
                    for (int i = 0; i < _pageCalculator.PamIntLength; i++)
                    {
                        var oldMask = *(asInt +i);
                        int newMask;

                        if ((oldMask & 0xFFFF) == 0)
                        {

                            newMask = oldMask | 0xFFFF;
                            if (Interlocked.CompareExchange(ref *(asInt + i), newMask, oldMask) == oldMask)
                            {
                                recordTaken = i * 2;
                                break;
                            }
                        }
                        if ((i != _pageCalculator.PamIntLength - 1 || _pageCalculator.PamSize % 4 == 0) &&
                            ((uint) oldMask & 0xFFFF0000) == 0)
                        {
                            unchecked
                            {
                                newMask = oldMask | (int) 0xFFFF0000;
                            }
                            if (Interlocked.CompareExchange(ref *(asInt + i), newMask, oldMask) == oldMask)
                            {
                                recordTaken = i * 2 + 1;
                                break;
                            }
                        }
                    }
                });
            if (recordTaken == -1)
              return -1;
            Interlocked.Increment(ref _usedRecords);
        
            return (short)CalcShiftFromPosition(recordTaken);
        }

        private volatile int _reordering  =0;
        private volatile int _syncer = 0;



        public unsafe bool IsRecordFree(ushort persistentRecordNum)
        {
            var pos = CalcPositionFromShift(persistentRecordNum);
            bool isFree = false;
            _accessor.QueueByteArrayOperation(0, _pageCalculator.PamSize,
                b =>
                {
                    var s = (ushort*) b;
                    var d = *(s+ pos);
                    isFree = (d & 0xFFFF) == 0;
                   
                });
            return isFree;
        }

        private static ushort FixUshort(ushort num) =>(ushort)( num >> 8 | num << 8);

        public unsafe void FreeRecord(ushort persistentRecordNum)
        {
            var pos = CalcPositionFromShift(persistentRecordNum);
           
            int change;
            _accessor.QueueByteArrayOperation(0, _pageCalculator.PamSize,
                b =>
                {
                    var s = (int*)b;
                    int d;
                    do
                    {
                        d = *(s + pos/2);
                        unchecked
                        {
                            if (pos % 2 == 1)
                            {
                                change = 0xFFFF & d;
                            }
                            else
                            {
                                change = (int) 0xFFFF0000 & d;
                            }
                        }
                        if (change == d)
                            return;
                    } while (Interlocked.CompareExchange(ref *(s + pos/2), change, d) != d);
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

        public unsafe void ApplyOrder(ushort[] recordsInOrder)
        {
            try
            {
                while (Interlocked.CompareExchange(ref _reordering, 1, 0) != 0) ;
                _accessor.QueueByteArrayOperation(0, _pageCalculator.PamSize,
                    b =>
                    {
                        var s = (int*)b;
                        int d;
                        for (ushort i = 1; i <= recordsInOrder.Length; i++)
                        {
                            var p = CalcPositionFromShift(recordsInOrder[i - 1]);
                            int oldMap, newMap;

                            do
                            {
                                oldMap =*(s+p / 2);
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
                            } while (Interlocked.CompareExchange(ref *(s + p / 2), newMap, oldMap) !=
                                     oldMap);
                        }
                    });

            }
            finally
            {
                _reordering = 0;
            }
          
        }

        public unsafe void DropOrder(ushort persistentRecordNum)
        {
            var p = CalcPositionFromShift(persistentRecordNum);
            int oldMap, newMap;
            _accessor.QueueByteArrayOperation(0, _pageCalculator.PamIntLength,
                b =>
                {
                    var s = (int*)b;
                    do
                    {                                             
                        oldMap = *(s + p / 2);
                        unchecked
                        {

                            if (p % 2 == 0)
                            {
                                newMap = oldMap | (int) 0xFFFF0000;
                            }
                            else
                            {
                                newMap = oldMap & 0xFFFF;
                            }
                        }
                    } while (Interlocked.CompareExchange(ref *(s + p / 2), newMap, oldMap) != oldMap);
                });
        }


        public void Compact()
        {
         
        }

        public int TotalUsedSize => _usedRecords * _fixedRecordSize + _pageCalculator.PamSize;
    }
}
