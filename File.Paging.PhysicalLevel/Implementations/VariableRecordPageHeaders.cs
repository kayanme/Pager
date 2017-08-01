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
        
        
        private readonly bool _slotArrayApplied;
        protected override int[] _recordInfo { get; }
        public VariableRecordPageHeaders(IPageAccessor accessor,bool slotInfoApplied)
        {
            _pageSize = accessor.PageSize;
            var _page = accessor.GetByteArray(0, accessor.PageSize);
        
             _accessor = accessor;
            _slotArrayApplied = slotInfoApplied;
            _recordInfo = ScanForHeaders(_page);


        }
        //формат записи - 4 бита тип (0 - пустой тип), 12 бит - размер записи
        //если включена информация о логическом порядке - ещё 16 бит на логический номер записи
     
        protected override void SetFree(ushort record)
        {
            if (RecordSize(record) == 0)
                return;
            var recordPosition =RecordShift(record)-(_slotArrayApplied?4: 2);

            var val =  RecordSize(record);
            var toWrite = new byte[] { (byte)(val >> 8)};
            _accessor.SetByteArray(toWrite, recordPosition, 1);
        }
        private ushort _totalRecords = 0;
        protected override ushort TotalRecords => _totalRecords;
        private int _firstFreeRecord;
        private int SlotArraySize => _firstFreeRecord * 2;
        protected  int[] ScanForHeaders(byte[] page)
        {
            ushort physicalRecordNum = 0;
            ushort logicalPosition = 0;
            _firstFreeRecord = -1;
            var records =new int[page.Length / 4+4];
            for (ushort i = 0;i<page.Length;)
            {
                if (i + 1 == page.Length)
                    break;
                var header = page[i]<<8 | page[i+1] ;
               
                if (header == 0)
                {
                    _firstFreeRecord = physicalRecordNum;
                    break;
                }
                if (_slotArrayApplied)
                {
                     logicalPosition = (ushort)(page[i + 2] << 8 | page[i + 3]);                    
                }
                else
                {
                    logicalPosition = physicalRecordNum;
                }
                byte type =(byte)(header >> 12);
                var size =(ushort)( header & 0x0FFF);
                var shift = _slotArrayApplied ? i +4: i + 2;
                if (type != 0)
                {                  
                    records[logicalPosition] =(FormRecordInf(type, size, (ushort)shift));                  
                    _totalRecords++;
                }
                else
                {
                    records[logicalPosition] =  0;
                }
                physicalRecordNum++;
                i +=(ushort)(size + (_slotArrayApplied ? i + 4 : i + 2));

            }          
            return records.ToArray();            
        }
        protected override IEnumerable<int> PossibleRecordsToInsert()
        {
            if (_firstFreeRecord == -1 || _firstFreeRecord == _recordInfo.Length)
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
                var shift = 0;
                var rec = SetNewRecordInfo(size, type, shift,record);
                Interlocked.Increment(ref _firstFreeRecord);
                return rec;
            }
            else
            {
                SpinWait.SpinUntil(() => RecordShift((ushort)(record - 1)) != 0xFFFC);
                var shift = RecordShift((ushort)(record - 1)) + RecordSize((ushort)(record - 1));
                if (shift + size > _pageSize)
                    return ushort.MaxValue;
                var rec = SetNewRecordInfo(size, type, shift,record);
                Interlocked.Increment(ref _firstFreeRecord);
                return rec;
            }

        }

        private ushort SetNewRecordInfo(ushort size, byte type, int shift,ushort record)
        {
            var val = type << 12 | size;
            if (!_slotArrayApplied)
            {
                var toWrite = new byte[] { (byte)(val >> 8), (byte)(val & 0xFF) };
                _accessor.SetByteArray(toWrite, shift, 2);
                
                return (ushort)(shift + 2);
            }
            else
            {
                var toWrite = new byte[] { (byte)(val >> 8), (byte)(val & 0xFF), (byte)(record >> 8), (byte)(record & 0xFF) };
                _accessor.SetByteArray(toWrite, shift, 4);                
                return (ushort)(shift + 4);
            }
        }

        protected override void UpdateUsed(ushort record, ushort shift, ushort size, byte type)
        {
             SetNewRecordInfo(size, type, shift, record);
        }

        protected override void SetNewLogicalRecordNum(ushort logicalRecordNum, ushort shift)
        {
            if (!_slotArrayApplied)
              base.SetNewLogicalRecordNum(logicalRecordNum, shift);
            else
            {
                var toWrite = new byte[] { (byte)(logicalRecordNum >> 8), (byte)(logicalRecordNum & 0xFF) };
                _accessor.SetByteArray(toWrite, shift-2, 2);
            }
        }
    }
}
