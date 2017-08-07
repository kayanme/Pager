using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public sealed class ComplexRecordTypePage<TRecordType> :  IPage<TRecordType>, IPhysicalLevelManipulation where TRecordType : TypedRecord, new()
    {

        private readonly IPageHeaders _headers;
        private readonly IPageAccessor _accessor;
     
        private readonly VariableRecordTypePageConfiguration<TRecordType> _config;
        internal ComplexRecordTypePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize,byte pageType, VariableRecordTypePageConfiguration<TRecordType> config)
        {
            Reference = reference;
            _headers  = headers;
            _accessor = accessor;
            _config = config;
            RegisteredPageType = pageType;
        }

        public IEnumerable<TRecordType> IterateRecords()
        {
            foreach (var i in _headers.NonFreeRecords())
            {
                var t = GetRecord(new PageRecordReference { LogicalRecordNum = i, Page = Reference });
                if (t != null)
                    yield return t;

            }
        }

        private void SetRecord<TType>(ushort offset, TType record,RecordDeclaration<TType> config) where TType : TRecordType
        {
            var recordStart = _headers.RecordShift(offset);
            var recordSize = _headers.RecordSize(offset);
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
            var config = FindConfig<TType>(out byte t);
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
            type.Reference.LogicalRecordNum = record;
            return true;
        }

        public TRecordType GetRecord(PageRecordReference reference) 
        {
            if (Reference != reference.Page)
                throw new ArgumentException("The record is on another page");

            if (!_headers.IsRecordFree((ushort)reference.LogicalRecordNum))
            {
                var offset = _headers.RecordShift((ushort)reference.LogicalRecordNum);
                var size = _headers.RecordSize((ushort)reference.LogicalRecordNum);
                var type = _headers.RecordType((ushort)reference.LogicalRecordNum);
                var bytes = _accessor.GetByteArray(offset, size);
                var r = new TRecordType()
                {
                    Reference = reference
                };
                var config = _config.RecordMap[type] as VariableSizeRecordDeclaration<TRecordType>;
                config.FillFromBytes(bytes, r);
                return r;
            }
            return null;
        }

        public void StoreRecord(TRecordType record)
        {
            if (record.Reference.Page != Reference)
                throw new ArgumentException();
            if (record.Reference.LogicalRecordNum == -1)
                throw new ArgumentException();
            var mapKey = _config.GetRecordType(record);
            var config = _config.RecordMap[mapKey];
            if (_headers.RecordSize((ushort)record.Reference.LogicalRecordNum) < config.GetSize(record))
                throw new ArgumentException("Record size is more, than slot space available");
            SetRecord((ushort)record.Reference.LogicalRecordNum, record, config);
        }

        public void FreeRecord(TRecordType record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (record.Reference.LogicalRecordNum == -1)
                throw new ArgumentException("Trying to delete deleted record");
            _headers.FreeRecord((ushort)record.Reference.LogicalRecordNum);
            record.Reference.LogicalRecordNum = -1;
        }

      

        public  double PageFullness => 0;

        public  PageReference Reference { get;  }

        public byte RegisteredPageType { get; }

        private bool _disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Flush();
                }

                _disposedValue = true;
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
        public void SwapRecords(PageRecordReference record1, PageRecordReference record2)
        {
            CheckReferenceToPageAffinity(record1);
            CheckReferenceToPageAffinity(record2);
            if (record1.LogicalRecordNum == -1)
                throw new ArgumentException("record1 was deleted");
            if (record2.LogicalRecordNum == -1)
                throw new ArgumentException("record2 was deleted");
            _headers.SwapRecords((ushort)record1.LogicalRecordNum, (ushort)record2.LogicalRecordNum);
        }

        public void Compact()
        {
            throw new NotImplementedException();
        }
    }
}
