using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Linq;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Implementations.Headers
{
    internal sealed class VariableRecordPageHeaders : PageHeadersBase
    {
        readonly IPageAccessor _accessor;
        readonly int _pageSize;
        
        
        
        protected override int[] RecordInfo { get; }
        public VariableRecordPageHeaders(IPageAccessor accessor)
        {
            _pageSize = accessor.PageSize;
            var page = accessor.GetByteArray(0, accessor.PageSize);
        
             _accessor = accessor;            
            RecordInfo = ScanForHeaders(page);


        }
        //формат записи - 4 бита тип (0 - пустой тип), 12 бит - размер записи
        //если включена информация о логическом порядке - ещё 16 бит на логический номер записи
     
        protected override void SetFree(ushort record)
        {
            if (RecordSize(record) == 0)
                return;
            var recordPosition =RecordShift(record)-HeaderOverheadSize;

            var val =  RecordSize(record);
            var toWrite = new byte[] { (byte)(val >> 8)};
            _accessor.SetByteArray(toWrite, recordPosition, 1);
        }
      
        private int _firstFreeRecord;
        private int SlotArraySize => _firstFreeRecord * 2;
        private  int[] ScanForHeaders(byte[] page)
        {
            ushort physicalRecordNum = 0;
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
                ushort logicalPosition = 0;
              
                logicalPosition = physicalRecordNum;
                
                byte type =(byte)(header >> 12);
                var size =(ushort)( header & 0x0FFF);
                var shift = i+ HeaderOverheadSize;
                if (type != 0)
                {                  
                    records[logicalPosition] =(FormRecordInf(type, size, (ushort)shift));
                    TotalUsedSize += (size + HeaderOverheadSize);
                    TotalUsedRecords++;
                }
                else
                {
                    records[logicalPosition] =  0;
                }
                physicalRecordNum++;
                i +=(ushort)(size + HeaderOverheadSize);

            }          
            return records.ToArray();            
        }
        protected override IEnumerable<int> PossibleRecordsToInsert()
        {
            if (_firstFreeRecord == -1 || _firstFreeRecord == RecordInfo.Length)
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




        protected override ushort SetUsed(ushort record, ushort size)
        {
           
            if (record == 0)
            {
                var shift = 0;
                var rec = SetNewRecordInfo(size, 0, shift,record);
                Interlocked.Increment(ref _firstFreeRecord);
                return rec;
            }
            else
            {
                SpinWait.SpinUntil(() => RecordShift((ushort)(record - 1)) != 0xFFFC);
                var shift = RecordShift((ushort)(record - 1)) + RecordSize((ushort)(record - 1));
                if (shift + size > _pageSize)
                    return ushort.MaxValue;
                var rec = SetNewRecordInfo(size, 0, shift,record);
                Interlocked.Increment(ref _firstFreeRecord);
                return rec;
            }

        }

        private ushort SetNewRecordInfo(ushort size, byte type, int shift, ushort record)
        {
            var val = type << 12 | size;
            //TODO: Remove type
            var toWrite = new byte[] {(byte) (val >> 8), (byte) (val & 0xFF)};
            _accessor.SetByteArray(toWrite, shift, 2);

            return (ushort) (shift + 2);

        }

        protected override void UpdateUsed(ushort record, ushort shift, ushort size)
        {
             SetNewRecordInfo(size, 0, shift, record);
        }
        

        protected override int HeaderOverheadSize => 2;

        public override void Compact()
        {
            throw new System.NotImplementedException();
        }
    }
}
