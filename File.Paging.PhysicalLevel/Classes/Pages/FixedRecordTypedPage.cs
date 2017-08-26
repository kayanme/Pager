using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class FixedRecordTypedPage<TRecordType> : TypedPageBase<TRecordType>, 
        ILogicalRecordOrderManipulation where TRecordType : TypedRecord, new()
    {
      
        private readonly FixedRecordTypePageConfiguration<TRecordType> _config;
        internal FixedRecordTypedPage(IPageHeaders headers, IPageAccessor accessor, PageReference reference,
            int pageSize, FixedRecordTypePageConfiguration<TRecordType> config,
            byte pageType):base(headers,accessor,reference,pageType,pageSize)
        {
        
            _config = config;
        }              
       

        public override TRecordType GetRecord(PageRecordReference reference)
        {
            CheckReferenceToPageAffinity(reference);
            if (reference is NullPageRecordReference)
                return null;
            
                Debug.Assert(reference is PhysicalPositionPersistentPageRecordReference, "reference is PhysicalPositionPersistentPageRecordReference");
                if (!Headers.IsRecordFree(reference.PersistentRecordNum))
                {                    
                    var offset = reference.PersistentRecordNum;
                    var size = Headers.RecordSize(reference.PersistentRecordNum);
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

        public override bool AddRecord(TRecordType type)
        {
          
                var physicalRecordNum = Headers.TakeNewRecord(0, (ushort) _config.RecordMap.GetSize);
                if (physicalRecordNum == -1)
                    return false;
                SetRecord(physicalRecordNum, type);
                if (type.Reference == null)
                    type.Reference = new PhysicalPositionPersistentPageRecordReference(Reference, (ushort)physicalRecordNum);
                return true;
          
        }

        private void SetRecord(int logicalRecordNum, TRecordType record)
        {
            var recordStart = Headers.RecordShift((ushort)logicalRecordNum);
            var recordSize = Headers.RecordSize((ushort)logicalRecordNum);
            var bytes = Accessor.GetByteArray(recordStart, recordSize);
            _config.RecordMap.FillBytes(record, bytes);
            Accessor.SetByteArray(bytes, recordStart, recordSize);
        }

        private void SetRecord(ushort physicalRecordNum, TRecordType record)
        {

            var recordSize = _config.RecordMap.GetSize;
            var bytes = Accessor.GetByteArray(physicalRecordNum, recordSize);
            _config.RecordMap.FillBytes(record, bytes);
            Accessor.SetByteArray(bytes, physicalRecordNum, recordSize);
        }

        public override void StoreRecord(TRecordType record)
        {
            if (record.Reference.Page != Reference)
                throw new ArgumentException();
          
                Debug.Assert(record.Reference is PhysicalPositionPersistentPageRecordReference, "record.Reference is PhysicalPositionPersistentPageRecordReference");
                SetRecord(record.Reference.PersistentRecordNum, record);
          
        }

        public override IEnumerable<PageRecordReference> IterateRecords()
        {
            foreach (var nonFreeRecord in Headers.NonFreeRecords())
            {
                yield return new PhysicalPositionPersistentPageRecordReference(Reference,nonFreeRecord);
            }
        }

        ~FixedRecordTypedPage()
        {
            Dispose(true);
        
        }

        public void ApplyOrder(PageRecordReference[] records)
        {
            Headers.ApplyOrder(records.Select(k=>k.PersistentRecordNum).ToArray());
        }

        public void DropOrder(PageRecordReference record)
        {
           Headers.DropOrder(record.PersistentRecordNum);
        }
    }
}
