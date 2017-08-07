using System;
using System.Collections.Generic;
using System.Diagnostics;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Implementations
{
    internal sealed class FixedRecordPageHeaders:PageHeadersBase
    {
        private const byte RecordUseMask = 0x80;
      
        private readonly ushort _fixedRecordSize;
   
        private readonly IPageAccessor _accessor;
        protected override int[] RecordInfo { get; }
        private int _lastKnownNotFree;
        

        public FixedRecordPageHeaders(IPageAccessor accessor,ushort recordSize):base()
        {
            TotalRecords =(ushort)( accessor.PageSize / (recordSize + 1));
            Debug.Assert(recordSize >= 3, "recordSize >= 3");
           
            _fixedRecordSize = recordSize;
            _accessor = accessor;
            
            RecordInfo = ScanForHeaders(accessor.GetByteArray(0, accessor.PageSize));
                      
        }

        protected override ushort TotalRecords { get; }

        private  int[] ScanForHeaders(byte[] page)
        {
            ushort recordNum = 0;
            var fullRecordSize = (ushort)(_fixedRecordSize + 1);
            var records = new int[(page.Length)/ fullRecordSize + 1];
           
            for (ushort i = 0; i< page.Length; i+= fullRecordSize)
            {
                if ((page[i] & RecordUseMask) == 0)
                {
                    records[recordNum] = FormRecordInf(0,0,0);                  
                }
                else
                {
                    checked
                    {
                        records[recordNum] = FormRecordInf(1, _fixedRecordSize, (ushort)(i+1));
                        _lastKnownNotFree = recordNum;
                    }               
                }
                recordNum++;
            }
           
            return records;
        }

        public override ushort RecordShift(ushort record)=> (ushort)(base.RecordShift(record)+1);

       


        protected override void SetFree(ushort record)
        {
            _accessor.SetByteArray(new[] { (byte)0 }, record * (_fixedRecordSize + 1), 1);
        }

        protected override  ushort SetUsed(ushort record, ushort size, byte type)
        {
            _accessor.SetByteArray(new[] { RecordUseMask }, record*(_fixedRecordSize+1), 1);
            checked
            {
                if (_lastKnownNotFree != -1)
                    _lastKnownNotFree = record;

                return (ushort)(record * (_fixedRecordSize + 1)+1);
            }
        }

        protected override IEnumerable<int> PossibleRecordsToInsert()
        {
            for (var i = _lastKnownNotFree == -1 ? 0 : _lastKnownNotFree; i < TotalRecords; i += 1)
            {
               yield return i;
            }
        }

        protected override void UpdateUsed(ushort record, ushort shift, ushort size, byte type)
        {
            throw new NotImplementedException();//оно и не должно использоваться, т.к. у записи фиксированного размера ничего не меняется
        }
    }
}
