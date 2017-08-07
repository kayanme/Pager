using System;
using System.Collections.Generic;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public sealed class FixedRecordTypedPage<TRecordType> :  IPage<TRecordType>, IPhysicalLevelManipulation where TRecordType : TypedRecord, new()
    {
        private readonly IPageAccessor _accessor;
        private readonly IPageHeaders _headers;
     
     
     
        private readonly FixedRecordTypePageConfiguration<TRecordType> _config;
        internal FixedRecordTypedPage(IPageHeaders headers, IPageAccessor accessor, PageReference reference,
            int pageSize, FixedRecordTypePageConfiguration<TRecordType> config,
            byte pageType)
        {
            Reference = reference;
            _accessor = accessor;
            _headers = headers;       
            _config = config;
            RegisteredPageType = pageType;
          
        }

        public  PageReference Reference { get; }

        public  double PageFullness =>0;

        public byte RegisteredPageType { get; }

        public IEnumerable<TRecordType> IterateRecords()
        {
            foreach (var i in _headers.NonFreeRecords())
            {                
                    var t = GetRecord(new PageRecordReference { LogicalRecordNum = i, Page = Reference });
                    if (t != null)
                        yield return t;
                
            }
        }

        public TRecordType GetRecord(PageRecordReference reference)
        {
            if (Reference != reference.Page)
                throw new ArgumentException("The record is on another page");
            if (reference.LogicalRecordNum == -1)
                return null;
            if (!_headers.IsRecordFree((ushort)reference.LogicalRecordNum))
            {
                var offset = _headers.RecordShift((ushort)reference.LogicalRecordNum);
                var size = _headers.RecordSize((ushort)reference.LogicalRecordNum);
                var bytes = _accessor.GetByteArray(offset, size);
                var r = new TRecordType()
                {
                    Reference = reference
                };
                _config.RecordMap.FillFromBytes(bytes, r);
                return r;
            }
            return null;
        }

        public bool AddRecord(TRecordType type)
        {
            var record = _headers.TakeNewRecord(1, (ushort)_config.RecordMap.GetSize);
            if (record == -1)
                return false;
            SetRecord(record, type);
            if (type.Reference == null)
                type.Reference = new PageRecordReference { Page = Reference };
            type.Reference.LogicalRecordNum = record;
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
            if (record.Reference.Page != Reference)
                throw new ArgumentException();
            SetRecord(record.Reference.LogicalRecordNum, record);

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

        public  void Flush()
        {
            _accessor.Flush();
        }

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
        ~FixedRecordTypedPage()
        {
            Dispose(true);
        }


        public  void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
