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
    internal sealed class FixedRecordPageHeaders:IPageHeaders
    {
        private const byte RecordUseMask = 0x80;
      
        private ushort _recordSize;
        private int _lastKnownNotFree;
        private IPageAccessor _accessor;

        private int[] _recordShifts;

        public FixedRecordPageHeaders(IPageAccessor accessor,ushort recordSize)
        {
            Debug.Assert(recordSize >= 3, "recordSize >= 3");
            var _page = accessor.GetByteArray(0, accessor.PageSize);
            _recordSize = recordSize;
            _accessor = accessor;
            ScanForHeaders(_page);
            ScanForLastNotFreeRecord();
        }

        public ushort RecordCount =>(ushort) _recordShifts.Count(k => k==-1);
          

        public byte HeaderSize => 1;

        private void ScanForHeaders(byte[] _page)
        {
            _recordShifts = new int[_accessor.PageSize / (_recordSize + HeaderSize)];
            for (ushort i = 0; i< _recordShifts.Length; i +=1)
            {
                if ((_page[RecordShift(i)] & RecordUseMask) == 0)
                    _recordShifts[i] = -1;
                else
                    _recordShifts[i] = RecordShift(i);               
            };
        }

        private void ScanForLastNotFreeRecord()
        {           
            _lastKnownNotFree = -1;
            for (int i =  (_recordShifts.Length-1);i>=0;i-=1)
            {
                if (!IsRecordFree((ushort)i))
                    break;
                _lastKnownNotFree = i;
            };
        }

        public void FreeRecord(ushort record)
        {
            Thread.BeginCriticalRegion();
            var r = _recordShifts[record];
            if (Interlocked.CompareExchange(ref _recordShifts[record], -1, r) == r)
            {                
                _accessor.SetByteArray(new[] { (byte)0 }, record * (_recordSize + HeaderSize), 1);
            }
            else
                throw new RecordWriteConflictException();          
            Thread.EndCriticalRegion();
           
        }

        private ushort RecordShift(ushort record) => (ushort)(record * (_recordSize + HeaderSize));

    
        public bool IsRecordFree(ushort record)
        {
            return _recordShifts[record] == -1;
        }

        public unsafe short TakeNewRecord()
        {
            Thread.BeginCriticalRegion();
            int i = 0;
            bool changed = false;

            for (i = _lastKnownNotFree == -1 ?0 : _lastKnownNotFree ; i < _recordShifts.Length; i += 1)
            {
                if (Interlocked.CompareExchange(ref _recordShifts[i], RecordUseMask, -1) == -1)
                {                
                    _accessor.SetByteArray(new[] { (byte)RecordUseMask }, RecordShift((ushort)i), 1);
                    break;
                }
            };
            Thread.EndCriticalRegion();
            if (i != _recordShifts.Length)
            {
                if (_lastKnownNotFree != -1)
                    _lastKnownNotFree = i ;
                return (short)(i);
            }
            else
            {               
                return -1;
            }
           
           
            
            
        }
    }
}
