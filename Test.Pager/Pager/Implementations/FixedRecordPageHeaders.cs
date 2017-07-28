using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pager;
using Pager.Contracts;
using Pager.Exceptions;
using Pager.Implementations;

namespace Pager.Implementations
{
    internal sealed class FixedRecordPageHeaders:PageHeadersBase
    {
        private const byte RecordUseMask = 0x80;
      
        private ushort _fixedRecordSize;
   
        private IPageAccessor _accessor;
        protected override int[] _recordInfo { get; }
        private int _lastKnownNotFree;
        private ushort _headerShift;

        public FixedRecordPageHeaders(IPageAccessor accessor,ushort headerShift,ushort recordSize):base()
        {
            TotalRecords =(ushort)( accessor.PageSize / (recordSize + 1));
            Debug.Assert(recordSize >= 3, "recordSize >= 3");
           
            _fixedRecordSize = recordSize;
            _accessor = accessor;
            _headerShift = headerShift;
            _recordInfo = ScanForHeaders(accessor.GetByteArray(0, accessor.PageSize), headerShift);
                      
        }

        protected override ushort TotalRecords { get; }

        protected override int[] ScanForHeaders(byte[] page,ushort headerShift)
        {
            ushort recordNum = 0;
            var fullRecordSize = (ushort)(_fixedRecordSize + 1);
            var records = new int[(page.Length - headerShift)/ fullRecordSize];
           
            for (ushort i = headerShift; i< page.Length; i+= fullRecordSize)
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
            };
           
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
            };
        }
    }
}
