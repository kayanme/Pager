using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Exceptions;

namespace File.Paging.PhysicalLevel.Implementations
{
    internal abstract class PageHeadersBase : IPageHeaders
    {
        private int _totalRecordSize;
        private int _totalUsedRecords;


        private const byte RecordUseMask = 0xFF;

        protected abstract  int[] RecordInfo { get; }

        public int TotalUsedSize
        {
            get { return _totalRecordSize; }
            protected set { _totalRecordSize = value; }
        }

        public ushort RecordCount => (ushort)_totalUsedRecords;            

        protected abstract IEnumerable<int> PossibleRecordsToInsert();

        protected abstract void SetFree(ushort record);

        protected abstract ushort SetUsed(ushort record, ushort size, byte type);

        protected abstract Task UpdateUsed(ushort record,ushort shift, ushort size, byte type);

        public async Task FreeRecord(ushort record)
        {
            await Task.Factory.StartNew(() =>
            {
                Thread.BeginCriticalRegion();
                var r = RecordInfo[record];
                var newInf = FormRecordInf(0, RecordSize(record), RecordShift(record));
                if (Interlocked.CompareExchange(ref RecordInfo[record], newInf, r) == r)
                {
                    SetFree(record);
                    Interlocked.Add(ref _totalRecordSize, -RecordSize(record) - HeaderOverheadSize);
                    Interlocked.Add(ref _totalUsedRecords, -1);
                }
                else
                    throw new RecordWriteConflictException();
                Thread.EndCriticalRegion();
            });
        }

        private const uint ShiftMask = 0xFFFC0000;
        private const uint SizeMask = 0x0003FFF0;
        private const uint TypeMask = 0x0000000F;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ushort RecordShift(ushort record) => (ushort)((RecordInfo[record] & ShiftMask) >> 18);//14 бит = 16384
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte RecordType(ushort record) => (byte)(RecordInfo[record] & TypeMask);//4 бит = 16
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort RecordSize(ushort record) => (ushort)((RecordInfo[record] & SizeMask) >> 4);//14 бит = 16384


        public async Task<bool> IsRecordFree(ushort record)
        {
            return await Task.Factory.StartNew(()=>RecordType(record)  == 0); 
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
        public async Task<short> TakeNewRecord(byte rType,ushort rSize)
        {
            return await Task.Factory.StartNew(() =>
            {
                Thread.BeginCriticalRegion();
                short index = -1;
                foreach (var i in PossibleRecordsToInsert())
                {
                    var it = FormRecordInf(rType, rSize, ushort.MaxValue);
                    if (Interlocked.CompareExchange(ref RecordInfo[i], it, 0) == 0)
                    {
                        var shift = SetUsed((ushort) i, rSize, rType);
                        if (shift == ushort.MaxValue) //если запись данного размера не влезает в свободое место
                        {
                            RecordInfo[i] = 0;
                            break;
                        }
                        else
                        {
                            RecordInfo[i] = FormRecordInf(rType, rSize, shift);
                            index = (short) i;
                            break;
                        }
                    }
                }
                Thread.EndCriticalRegion();
                if (index != -1)
                {
                    Interlocked.Add(ref _totalRecordSize, rSize + HeaderOverheadSize);
                    Interlocked.Add(ref _totalUsedRecords, 1);
                    return (short)index;
                }
                else
                {
                    return (short)-1;
                }
            });
        }

        public IEnumerable<ushort> NonFreeRecords()=>  RecordInfo.Where((k,i) => RecordType((ushort)i) != 0).Select((k, i) => (ushort)i);

        public async Task SetNewRecordInfo(ushort recordNum,ushort rSize, byte rType)
        {
            var oldInf = RecordInfo[recordNum];
            var shift = RecordShift(recordNum);
            var oldSize = RecordSize(recordNum);
            var t = FormRecordInf(rType, rSize, shift);
            if (Interlocked.CompareExchange(ref RecordInfo[recordNum], t, oldInf) != oldInf)
                throw new RecordWriteConflictException();
            Interlocked.Add(ref _totalRecordSize, rSize- oldSize);
            await UpdateUsed(recordNum, shift, rSize, rType);
        }

       // [MethodImpl(MethodImplOptions.Synchronized)]
        public async Task SwapRecords(ushort recordOne, ushort recordTwo)
        {
            if (RecordType(recordOne) == 0 || RecordType(recordTwo) == 0)
                throw new InvalidOperationException();

            var oldTwo = RecordInfo[recordTwo];
            var oldOne = Interlocked.Exchange(ref RecordInfo[recordOne], RecordInfo[recordTwo]);
            if (Interlocked.CompareExchange(ref RecordInfo[recordTwo], oldOne, oldTwo) != oldTwo)
            {
                Interlocked.CompareExchange(ref RecordInfo[recordOne], oldOne, oldTwo);
                throw new RecordWriteConflictException();
            }
            SetNewLogicalRecordNum(recordOne, RecordShift(recordOne));
            SetNewLogicalRecordNum(recordTwo, RecordShift(recordTwo));

        }

        public abstract Task Compact();
    }
}