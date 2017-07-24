using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pager.Contracts;

namespace Pager
{
    public abstract class TypedPage:IDisposable
    {
        public abstract PageReference Reference { get; }
        public abstract double PageFullness { get; }
        public abstract void Flush();
        public abstract void Dispose();
    }

    public unsafe sealed class FixedRecordTypedPage<TRecordType> : TypedPage where TRecordType : TypedRecord,new()
    {
        PageReference _reference;
        private IPageAccessor _accessor;
        private IPageHeaders _headers;
        private readonly int _recordSize;
        private readonly int _maxRecords;
        private int _headerSizeInBytes =>_headers.HeaderSize;
        private FixedRecordTypePageConfiguration<TRecordType> _config;
        internal FixedRecordTypedPage(IPageHeaders headers,IPageAccessor accessor, PageReference reference,int pageSize,FixedRecordTypePageConfiguration<TRecordType> config)
        {
            _reference = reference;
            _accessor = accessor;
            _headers = headers;
            _recordSize = config.RecordType.GetSize(null);
            _config = config;
            _maxRecords = pageSize / (_recordSize + _headerSizeInBytes);
        }

        public override PageReference Reference => _reference;

        public override double PageFullness => _headers.RecordCount / _maxRecords;

        public IEnumerable<TRecordType> IterateRecords()
        {
        
            for (ushort i=0;i<_maxRecords;i++)
            {
                if (!_headers.IsRecordFree(i))
                {
                    var t = GetRecord(new PageRecordReference { Record = i, Page = Reference });
                    if (t != null)
                        yield return t;
                }
            }
        }

        public TRecordType GetRecord(PageRecordReference reference)
        {
            if (Reference != reference.Page)
                throw new ArgumentException("The record is on another page");
            var offset = reference.Record * (_recordSize + _headerSizeInBytes);
            var bytes = _accessor.GetByteArray(reference.Record*(_recordSize + _headerSizeInBytes), _recordSize + _headerSizeInBytes);
            if (!_headers.IsRecordFree((ushort)reference.Record))
            {
                var r = new TRecordType();
                r.Reference = reference;
                _config.RecordType.FillFromBytes(new ArraySegment<byte>(bytes, 1, bytes.Length - 1),r);
                return r;
            }
            return null;
        }

        public bool AddRecord(TRecordType type)
        {
            var record = _headers.TakeNewRecord();
            if (record == -1)
                return false;
            SetRecord(record, type);
            if (type.Reference == null)
                type.Reference = new PageRecordReference { Page = Reference};
            type.Reference.Record = record;
            return true;
        }

        private void SetRecord(int offset,TRecordType record)
        {
            var recordStart = offset * (_recordSize + _headerSizeInBytes);
            var bytes = _accessor.GetByteArray(recordStart, _recordSize + _headerSizeInBytes);
            _config.RecordType.FillBytes(record,new ArraySegment<byte>(bytes, 1, bytes.Length - 1));
            _accessor.SetByteArray(bytes, recordStart, _recordSize + _headerSizeInBytes);
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

        public override void Flush()
        {
            _accessor.Flush();
        }

        public override void Dispose()
        {
            _accessor.Flush();
        }
    }
}
