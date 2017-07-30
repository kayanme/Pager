using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pager.Contracts;

namespace Pager.Implementations
{
    internal sealed class VariableRecordPageHeaders : PageHeadersBase
    {
        IPageAccessor _accessor;
        int _pageSize;
        private readonly ushort _headerShift;
        private ushort _newRecordBorder;
        protected override int[] _recordInfo { get; }
        public VariableRecordPageHeaders(IPageAccessor accessor,ushort headerShift)
        {
            _pageSize = accessor.PageSize;
            var _page = accessor.GetByteArray(0, accessor.PageSize);
            _headerShift = headerShift;
             _accessor = accessor;
            _recordInfo = ScanForHeaders(_page, headerShift);


        }
        //формат записи - 4 бита тип (0 - пустой тип), 12 бит - размер записи
     
        protected override void SetFree(ushort record)
        {
            var recordPosition =RecordShift(record);
            var toWrite = new byte[] { 0};
            _accessor.SetByteArray(toWrite, recordPosition, 1);
        }
        private ushort _totalRecords = 0;
        protected override ushort TotalRecords => _totalRecords;
        private int _firstFreeRecord;

        protected override int[] ScanForHeaders(byte [] page,ushort headerShift)
        {
            ushort recordNum = 0;
            _firstFreeRecord = -1;
            var records =new int[page.Length / 4];
            for (ushort i = headerShift;i<page.Length;)
            {
                if (i + 1 == page.Length)
                    break;
                var header = page[i]<<8 | page[i+1] ;
                if (header == 0)
                {
                    _firstFreeRecord = recordNum;
                    break;
                }
                byte type =(byte)(header >> 12);
                var size =(ushort)( header & 0x0FFF);
                if (type != 0)
                {                  
                    records[recordNum]=(FormRecordInf(type, (ushort)size, (ushort)(i+2)));                  
                    _totalRecords++;
                }
                else
                {
                    records[recordNum] =  0;
                }
                recordNum++;
                i +=(ushort)(size + 2);

            }
           return records.ToArray();
        }
        protected override IEnumerable<int> PossibleRecordsToInsert()
        {
            if (_firstFreeRecord == -1)
                yield break;
            var cuBorder = 0;
            for (ushort i =(ushort) _firstFreeRecord; cuBorder < _pageSize;)
            {
                yield return i;
                SpinWait.SpinUntil(() => RecordShift(i) != 0xFFFC);
                cuBorder = RecordShift(i) + RecordSize(i);
                i += 1;
            }
        }




        protected override ushort SetUsed(ushort record, ushort size, byte type)
        {
           
            if (record == 0)
            {
                var shift = _headerShift;
                var val = type << 12 | size;
                var toWrite = new byte[] {(byte)(val>>8),(byte)(val & 0xFF) };
                _accessor.SetByteArray(toWrite, shift, 2);
                Interlocked.Increment(ref _firstFreeRecord);
                return (ushort)(shift + 2);
            }
            else
            {
                SpinWait.SpinUntil(() => RecordShift((ushort)(record - 1))!= 0xFFFC);
                var shift = RecordShift((ushort)(record - 1))+ RecordSize((ushort)(record - 1));
                if (shift + size  > _pageSize)
                    return ushort.MaxValue;
                var val = type << 12 | size;
                var toWrite = new byte[] { (byte)(val >> 8), (byte)(val & 0xFF) };
                _accessor.SetByteArray(toWrite, shift, 2);

                Interlocked.Increment(ref _firstFreeRecord);
                return (ushort)(shift+2);
            }
          
        }


        protected override void UpdateUsed(ushort record, ushort shift, ushort size, byte type)
        {
            var val = type << 12 | size;
            var toWrite = new byte[] { (byte)(val >> 8), (byte)(val & 0xFF) };
            _accessor.SetByteArray(toWrite, shift, 2);
        }
    }
}
