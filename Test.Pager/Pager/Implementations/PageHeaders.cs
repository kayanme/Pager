using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pager;
using Pager.Contracts;
using Pager.Implementations;

namespace Pager.Implementations
{
    internal sealed class FixedRecordPageHeaders:IPageHeaders
    {
        private const byte RecordUseMask = 0x80;
        private byte[] _page;
        private int _recordSize;
        private IPageAccessor _accessor;
        public FixedRecordPageHeaders(IPageAccessor accessor,int recordSize)
        {
            Debug.Assert(recordSize >= 3, "recordSize >= 3");
            _page = accessor.GetByteArray(0, accessor.PageSize);
            _recordSize = recordSize;
            _accessor = accessor;
        }

        public int RecordCount=>Enumerable.Range(0, _accessor.PageSize / (_recordSize + HeaderSize)).Select(IsRecordFree).Count(k => !k);
          

        public int HeaderSize => 1;

        public void FreeRecord(int record)
        {
            Thread.BeginCriticalRegion();
            _page[record * (_recordSize + HeaderSize)] = 0;
            _accessor.SetByteArray(new[] { (byte)0 }, record * (_recordSize + HeaderSize), 1); 
            Thread.EndCriticalRegion();
           
        }

        public bool IsRecordFree(int record)
        {
            return (_page[record * (_recordSize + HeaderSize)] & RecordUseMask) == 0;
        }

        public unsafe int TakeNewRecord()
        {
            Thread.BeginCriticalRegion();
            int i = 0;
            bool changed = false;
            for (i=0; i < _page.Length ;i+=_recordSize+HeaderSize)
            {
                fixed (byte* a = &_page[i])
                {
                    if (Interlocked.CompareExchange(ref *(int*)a, RecordUseMask, 0) == 0)
                        break; ;
                }
            };
            if (i < _page.Length)
            {
                _accessor.SetByteArray(new[] { RecordUseMask }, i, 1);
                Thread.EndCriticalRegion();
                return i / (_recordSize + HeaderSize);
            }
            else
            {
                Thread.EndCriticalRegion();
                return -1;
            }
            
            
        }
    }
}
