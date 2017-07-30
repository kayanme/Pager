using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Contracts;

namespace Pager.Classes
{
    public sealed class FixedRecordTypedPage<TRecordType> :  IPage<TRecordType> where TRecordType : TypedRecord, new()
    {
        PageReference _reference;
        private IPageAccessor _accessor;
        private IPageHeaders _headers;
     
     
     
        private FixedRecordTypePageConfiguration<TRecordType> _config;
        internal FixedRecordTypedPage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize, FixedRecordTypePageConfiguration<TRecordType> config)
        {
            _reference = reference;
            _accessor = accessor;
            _headers = headers;       
            _config = config;
          
        }

        public  PageReference Reference => _reference;

        public  double PageFullness =>0;

        public IEnumerable<TRecordType> IterateRecords()
        {
            foreach (var i in _headers.NonFreeRecords())
            {
                
                    var t = GetRecord(new PageRecordReference { Record = i, Page = Reference });
                    if (t != null)
                        yield return t;
                
            }
        }

        public TRecordType GetRecord(PageRecordReference reference)
        {
            if (Reference != reference.Page)
                throw new ArgumentException("The record is on another page");
           
            if (!_headers.IsRecordFree((ushort)reference.Record))
            {
                var offset = _headers.RecordShift((ushort)reference.Record);
                var size = _headers.RecordSize((ushort)reference.Record);
                var bytes = _accessor.GetByteArray(offset, size);
                var r = new TRecordType();
                r.Reference = reference;
                _config.RecordMap.FillFromBytes(bytes, r);
                return r;
            }
            return null;
        }

        public bool AddRecord(TRecordType type)
        {
            var record = _headers.TakeNewRecord(0, (ushort)_config.RecordMap.GetSize);
            if (record == -1)
                return false;
            SetRecord(record, type);
            if (type.Reference == null)
                type.Reference = new PageRecordReference { Page = Reference };
            type.Reference.Record = record;
            return true;
        }

        private void SetRecord(int offset, TRecordType record)
        {
            var recordStart = _headers.RecordShift((ushort)offset);
            var recordSize = _headers.RecordSize((ushort)offset);
            var bytes = _accessor.GetByteArray(recordStart, recordSize);
            _config.RecordMap.FillBytes(record, bytes);
            _accessor.SetByteArray(bytes, recordStart, recordSize);
        }

        public void StoreRecord(TRecordType record)
        {
            if (record.Reference.Page != this.Reference)
                throw new ArgumentException();
            SetRecord(record.Reference.Record, record);

        }

        public void FreeRecord(TRecordType record)
        {
            if (record.Reference.Record == -1)
                throw new ArgumentException("Trying to delete deleted record");
            _headers.FreeRecord((ushort)record.Reference.Record);
            record.Reference.Record = -1;
        }

        public  void Flush()
        {
            _accessor.Flush();
        }

        private bool disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Flush();
                }

                disposedValue = true;
            }
        }
        ~FixedRecordTypedPage()
        {
            Dispose(true);
        }


        public  void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
