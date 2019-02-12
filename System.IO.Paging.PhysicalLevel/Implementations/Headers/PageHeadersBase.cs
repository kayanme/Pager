using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Exceptions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Implementations.Headers
{
    internal abstract class PageHeadersBase : IPageHeaders
    {
        private int _totalRecordSize;
        private int _totalUsedRecords;


        protected abstract  int[] RecordInfo { get; }

        public int TotalUsedSize
        {
            get => _totalRecordSize;
            protected set => _totalRecordSize = value;
        }

        public ushort RecordCount => (ushort)_totalUsedRecords;            

        protected abstract IEnumerable<int> PossibleRecordsToInsert();

        protected abstract void SetFree(ushort record);

        protected abstract ushort SetUsed(ushort record, ushort size);

        protected abstract void UpdateUsed(ushort record,ushort shift, ushort size);

        public void FreeRecord(ushort record)
        {

            Thread.BeginCriticalRegion();
            var r = RecordInfo[record];
            var newInf = FormRecordInf(0, RecordSize(record), RecordShift(record));
            if (Interlocked.CompareExchange(ref RecordInfo[record], newInf, r) == r)
            {
                SetFree(record);
                Interlocked.Add(ref _totalRecordSize, -RecordSize(record)- HeaderOverheadSize);
                Interlocked.Add(ref _totalUsedRecords, -1);
            }
            else
                throw new RecordWriteConflictException();
            Thread.EndCriticalRegion();

        }

        private const uint ShiftMask = 0b11111111111111000000000000000000;
        private const uint SizeMask =  0b00000000000000111111111111110000;
        private const uint TypeMask =  0b00000000000000000000000000001111;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ushort RecordShift(ushort record) => (ushort)((RecordInfo[record] & ShiftMask) >> 18);//14 bits = 16384
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort RecordSize(ushort record) => (ushort)((RecordInfo[record] & SizeMask) >> 4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort RecordType(ushort record) => (ushort)((RecordInfo[record] & TypeMask));


        public bool IsRecordFree(int logicalRecordNum)
        {
            return RecordType((ushort)logicalRecordNum) == 0;
        }

        public bool IsRecordFree(ushort physicalRecordNum)
        {
            if (physicalRecordNum == 0)
            {
                return RecordType(physicalRecordNum) ==0;
                var fst = RecordInfo.First(k => (k & ShiftMask )>> 18 == 0);
                return (fst & SizeMask) >> 4 == 0;
                
            }
            return RecordInfo.Where(k => k != 0).Any(k2=>((k2 & ShiftMask) >> 18 == physicalRecordNum) && ((k2 & SizeMask) >> 4 != 0));
        }

        public ushort TotalUsedRecords
        {
            get => (ushort)_totalUsedRecords; protected set => _totalUsedRecords = value;
        }

        protected virtual void SetNewLogicalRecordNum(ushort logicalRecordNum,ushort shift)
        {
            throw new InvalidOperationException("Not supported");
        }

        protected int FormRecordInf(byte rType, ushort rSize, ushort rShift) => (rShift << 18) | (rSize << 4) | (rType);
        protected abstract int HeaderOverheadSize {get;}
        public short TakeNewRecord(ushort rSize)
        {
         
            Thread.BeginCriticalRegion();                     
            short index = -1;
            foreach (var i in PossibleRecordsToInsert())
            {
                var it = FormRecordInf(0, rSize, ushort.MaxValue);
                if (Interlocked.CompareExchange(ref RecordInfo[i], it, 0) == 0)
                {                    
                    var shift = SetUsed((ushort)i, rSize);
                    if (shift == ushort.MaxValue)//если запись данного размера не влезает в свободое место
                    {
                        RecordInfo[i] = 0;
                        break;                    
                    }                  
                    else
                    {
                        RecordInfo[i] = FormRecordInf(0, rSize, shift);
                        index = (short)i;
                        break;
                    }
                }
            }
            Thread.EndCriticalRegion();
            if (index!=-1)
            {
                Interlocked.Add(ref _totalRecordSize,rSize+ HeaderOverheadSize);
                Interlocked.Add(ref _totalUsedRecords, 1);
                return index;
            }
            else
            {
                return -1;
            }

        }

        public IEnumerable<ushort> NonFreeRecords() 
            =>  RecordInfo.Where((k,i) => RecordType((ushort)i) != 0).Select((k, i) => (ushort)i);

        public void SetNewRecordInfo(ushort logicalRecordNum,ushort rSize)
        {
            var oldInf = RecordInfo[logicalRecordNum];
            var shift = RecordShift(logicalRecordNum);
            var oldSize = RecordSize(logicalRecordNum);
            var t = FormRecordInf(0, rSize, shift);
            if (Interlocked.CompareExchange(ref RecordInfo[logicalRecordNum], t, oldInf) != oldInf)
                throw new RecordWriteConflictException();
            Interlocked.Add(ref _totalRecordSize, rSize- oldSize);
            UpdateUsed(logicalRecordNum, shift, rSize);
        }

        public void ApplyOrder(ushort[] recordsInOrder)
        {
            throw new NotImplementedException();
        }

        public void DropOrder(ushort persistentRecordNum)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual void SwapRecords(ushort recordOne, ushort recordTwo)
        {
            throw new InvalidOperationException("Not supported");

        }

        public abstract void Compact();
    }
}