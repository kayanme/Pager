using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Implementations
{
    internal sealed class FixedRecordPageHeaders:PageHeadersBase
    {
        private const byte RecordUseMask = 0x80;
      
        private readonly ushort _fixedRecordSize;
   
        private readonly IPageAccessor _accessor;
        protected override int[] RecordInfo { get; }
        private readonly ConcurrentSortedSet<ushort> _freeRecordNumbers = new ConcurrentSortedSet<ushort>();
     

        public FixedRecordPageHeaders(IPageAccessor accessor,ushort recordSize):base()
        {
          
            Debug.Assert(recordSize >= 3, "recordSize >= 3");
           
            _fixedRecordSize = recordSize;
            _accessor = accessor;
            
            RecordInfo = ScanForHeaders(accessor.GetByteArray(0, accessor.PageSize));
                      
        }

      

        private  int[] ScanForHeaders(byte[] page)
        {
            ushort recordNum = 0;
            var fullRecordSize = (ushort)(_fixedRecordSize + HeaderOverheadSize);
            var records = new int[(page.Length)/ fullRecordSize + 1];
           
            for (ushort i = 0; i< page.Length-fullRecordSize+1; i+= fullRecordSize)
            {
                if ((page[i] & RecordUseMask) == 0)
                {
                    records[recordNum] = FormRecordInf(0,0,0);         
                    _freeRecordNumbers.Add(recordNum);
                }
                else
                {
                    checked
                    {
                        records[recordNum] = FormRecordInf(1, _fixedRecordSize, (ushort)(i+ HeaderOverheadSize));                      
                        TotalUsedSize += _fixedRecordSize+ HeaderOverheadSize;
                        TotalUsedRecords++;
                    }               
                }
                recordNum++;
            }
           
            return records;
        }

        public override ushort RecordShift(ushort record)=> (ushort)(base.RecordShift(record)+ HeaderOverheadSize);

        protected override int HeaderOverheadSize => 1;

        public override void Compact()
        {
           
        }

        protected override void SetFree(ushort record)
        {
            _accessor.SetByteArray(new[] { (byte)0 }, record * (_fixedRecordSize + HeaderOverheadSize), 1);
        }

        protected override  ushort SetUsed(ushort record, ushort size, byte type)
        {
            _accessor.SetByteArray(new[] { RecordUseMask }, record*(_fixedRecordSize+ HeaderOverheadSize), 1);
            checked
            {               

                return (ushort)(record * (_fixedRecordSize + HeaderOverheadSize) + HeaderOverheadSize);
            }
        }

        protected override IEnumerable<int> PossibleRecordsToInsert()
        {
           while(_freeRecordNumbers.TryTakeMin(out var free))
            {
                yield return free;
                _freeRecordNumbers.Add(free);
            }
        }

        protected override Task UpdateUsed(ushort record, ushort shift, ushort size, byte type)
        {
            throw new NotImplementedException();//оно и не должно использоваться, т.к. у записи фиксированного размера ничего не меняется
        }
    }
}
