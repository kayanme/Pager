using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pager.Contracts;

namespace Pager.Classes
{
    public sealed class ComplexRecordTypePage<TRecordType> :  IPage<TRecordType> where TRecordType : TypedRecord, new()
    {

        private IPageHeaders _headers;
        private IPageAccessor _accessor;
     
        private VariableRecordTypePageConfiguration<TRecordType> _config;
        internal ComplexRecordTypePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize, VariableRecordTypePageConfiguration<TRecordType> config)
        {
            Reference = reference;
            _headers  = headers;
            _accessor = accessor;
            _config = config;
        }


        private void SetRecord<TType>(ushort offset, TType record,RecordDeclaration<TType> config) where TType : TRecordType
        {
            var recordStart = _headers.RecordShift((ushort)offset);
            var recordSize = _headers.RecordSize((ushort)offset);
            var type = _config.GetRecordType(record);
            var bytes = _accessor.GetByteArray(recordStart, recordSize);
            config.FillBytes(record, bytes);

            Thread.BeginCriticalRegion();
            _headers.SetNewRecordInfo(offset,recordSize, type);
            _accessor.SetByteArray(bytes, recordStart, recordSize);
            Thread.EndCriticalRegion();
        }

        private VariableSizeRecordDeclaration<TType> FindConfig<TType>() where TType : TRecordType
        {
            byte t;
            var config = FindConfig<TType>(out t);
            return config;
        }

        private VariableSizeRecordDeclaration<TType> FindConfig<TType>(out byte type) where TType:TRecordType
        {
            var map = _config.RecordMap.FirstOrDefault(k => k.Value is VariableSizeRecordDeclaration<TType>);
            if (map.Value == null)
                throw new InvalidOperationException("No such type in page map");
            var config = map.Value as VariableSizeRecordDeclaration<TType>;
            type = map.Key;
            return config;
        }

        public bool AddRecord(TRecordType type)
        {
           
            var mapKey = _config.GetRecordType(type);
            var config = _config.RecordMap[mapKey];
            var record = _headers.TakeNewRecord(mapKey, (ushort)config.GetSize(type));
            if (record == -1)
                return false;
            SetRecord((ushort)record, type,config);
            if (type.Reference == null)
                type.Reference = new PageRecordReference { Page = Reference };
            type.Reference.Record = record;
            return true;
        }

        public TRecordType GetRecord(PageRecordReference reference) 
        {
            if (Reference != reference.Page)
                throw new ArgumentException("The record is on another page");

            if (!_headers.IsRecordFree((ushort)reference.Record))
            {
                var offset = _headers.RecordShift((ushort)reference.Record);
                var size = _headers.RecordSize((ushort)reference.Record);
                var type = _headers.RecordType((ushort)reference.Record);
                var bytes = _accessor.GetByteArray(offset, size);
                var r = new TRecordType();
                r.Reference = reference;             
                var config = _config.RecordMap[type] as VariableSizeRecordDeclaration<TRecordType>;
                config.FillFromBytes(bytes, r);
                return r;
            }
            return null;
        }

        public void StoreRecord(TRecordType record)
        {
            if (record.Reference.Page != this.Reference)
                throw new ArgumentException();
            if (record.Reference.Record == -1)
                throw new ArgumentException();
            var mapKey = _config.GetRecordType(record);
            var config = _config.RecordMap[mapKey];
            if (_headers.RecordSize((ushort)record.Reference.Record) < config.GetSize(record))
                throw new ArgumentException("Record size is more, than slot space available");
            SetRecord((ushort)record.Reference.Record, record, config);
        }

        public void FreeRecord(TRecordType record)
        {
            if (record == null)
                throw new ArgumentNullException("record");
            if (record.Reference.Record == -1)
                throw new ArgumentException("Trying to delete deleted record");
            _headers.FreeRecord((ushort)record.Reference.Record);
            record.Reference.Record = -1;
        }

      

        public  double PageFullness => 0;

        public  PageReference Reference { get;  }

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
        ~ComplexRecordTypePage()
        {
            Dispose(true);
        }


        public  void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public  void Flush()
        {
            _accessor.Flush();
        }

        private void CheckReferenceToPageAffinity(PageRecordReference reference)
        {
            if (reference.Page != Reference)
                throw new ArgumentException("The record is on another page");
        }
        public void SwapRecords(TRecordType record1, TRecordType record2)
        {
            CheckReferenceToPageAffinity(record1.Reference);
            CheckReferenceToPageAffinity(record2.Reference);
            if (record1.Reference.Record == -1)
                throw new ArgumentException("record1 was deleted");
            if (record2.Reference.Record == -1)
                throw new ArgumentException("record2 was deleted");
            _headers.SwapRecords((ushort)record1.Reference.Record, (ushort)record2.Reference.Record);
        }
    }
}
