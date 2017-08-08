using System;
using System.Collections.Generic;
using System.Threading;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class FixedRecordTypedPage<TRecordType> : TypedPageBase,IPage<TRecordType> where TRecordType : TypedRecord, new()
    {
        private readonly int _pageSize;
        private readonly FixedRecordTypePageConfiguration<TRecordType> _config;
        internal FixedRecordTypedPage(IPageHeaders headers, IPageAccessor accessor, PageReference reference,
            int pageSize, FixedRecordTypePageConfiguration<TRecordType> config,
            byte pageType):base(headers,accessor,reference,pageType)
        {
            _pageSize = pageSize;
            _config = config;
        }
       
        public override double PageFullness =>0;

        public IEnumerable<TRecordType> IterateRecords()
        {
            foreach (var i in Headers.NonFreeRecords())
            {
               
                    var t = GetRecord(new PageRecordReference {LogicalRecordNum = i, Page = Reference});
                    if (t != null)
                        yield return t;
            }

        }

        public TRecordType GetRecord(PageRecordReference reference)
        {
            CheckReferenceToPageAffinity(reference);
            if (reference.LogicalRecordNum == -1)
                return null;
            try
            {
                CompactionLock.EnterReadLock();
                if (!Headers.IsRecordFree((ushort) reference.LogicalRecordNum))
                {
                    var offset = Headers.RecordShift((ushort) reference.LogicalRecordNum);
                    var size = Headers.RecordSize((ushort) reference.LogicalRecordNum);
                    var bytes = Accessor.GetByteArray(offset, size);
                    var r = new TRecordType()
                    {
                        Reference = reference
                    };
                    _config.RecordMap.FillFromBytes(bytes, r);
                    return r;
                }
                return null;
            }
            finally
            {
                CompactionLock.ExitReadLock();
            }
        }

        public bool AddRecord(TRecordType type)
        {
            try
            {
                CompactionLock.EnterReadLock();
                var record = Headers.TakeNewRecord(1, (ushort) _config.RecordMap.GetSize);
                if (record == -1)
                    return false;
                SetRecord(record, type);
                if (type.Reference == null)
                    type.Reference = new PageRecordReference {Page = Reference};
                type.Reference.LogicalRecordNum = record;
                return true;
            }
            finally
            {
                CompactionLock.ExitReadLock();
            }
        }

        private void SetRecord(int offset, TRecordType record)
        {
            var recordStart = Headers.RecordShift((ushort)offset);
            var recordSize = Headers.RecordSize((ushort)offset);
            var bytes = Accessor.GetByteArray(recordStart, recordSize);
            _config.RecordMap.FillBytes(record, bytes);
            Accessor.SetByteArray(bytes, recordStart, recordSize);
        }

        public  void StoreRecord(TRecordType record)
        {
            if (record.Reference.Page != Reference)
                throw new ArgumentException();
            try
            {
                CompactionLock.EnterReadLock();
                SetRecord(record.Reference.LogicalRecordNum, record);
            }
            finally
            {
                CompactionLock.ExitReadLock();
            }
        }

        public void FreeRecord(TRecordType record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (record.Reference.LogicalRecordNum == -1)
                throw new ArgumentException("Trying to delete deleted record");
            try
            {
                CompactionLock.EnterReadLock();
                Headers.FreeRecord((ushort) record.Reference.LogicalRecordNum);
                record.Reference.LogicalRecordNum = -1;
            }
            finally
            {
                CompactionLock.ExitReadLock();
            }
        }



        
       

        


        ~FixedRecordTypedPage()
        {
            Dispose(true);
        
        }
    }
}
