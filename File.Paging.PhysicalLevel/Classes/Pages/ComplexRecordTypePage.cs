using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class ComplexRecordTypePage<TRecordType> : TypedPageBase<TRecordType> where TRecordType : TypedRecord, new()
    {
       
        private readonly VariableRecordTypePageConfiguration<TRecordType> _config;
        internal ComplexRecordTypePage(IPageHeaders headers, IPageAccessor accessor, 
            PageReference reference, int pageSize,byte pageType, VariableRecordTypePageConfiguration<TRecordType> config):
            base(headers,accessor,reference,pageType,pageSize)
        {
         
            _config = config;
        }

       

        private void SetRecord<TType>(ushort offset, TType record,RecordDeclaration<TType> config) where TType : TRecordType
        {
            var recordStart = Headers.RecordShift(offset);
            var recordSize = Headers.RecordSize(offset);
            var type = _config.GetRecordType(record);
            var bytes = Accessor.GetByteArray(recordStart, recordSize);
            config.FillBytes(record, bytes);

            Thread.BeginCriticalRegion();
            Headers.SetNewRecordInfo(offset,recordSize, type);
            Accessor.SetByteArray(bytes, recordStart, recordSize);
            Thread.EndCriticalRegion();
        }
            

        public override bool AddRecord(TRecordType type)
        {
           
            var mapKey = _config.GetRecordType(type);
            var config = _config.RecordMap[mapKey];
            var record = Headers.TakeNewRecord(mapKey, (ushort)config.GetSize(type));
            if (record == -1)
                return false;
            SetRecord((ushort)record, type,config);
            if (type.Reference == null)
                type.Reference = _config.WithLogicalSort?
                    (PageRecordReference)new RowKeyPersistentPageRecordReference(Reference,0)
                                       : new LogicalPositionPersistentPageRecordReference (Reference, (ushort)record);
         
            return true;
        }

        public override TRecordType GetRecord(PageRecordReference reference) 
        {
            if (Reference != reference.Page)
                throw new ArgumentException("The record is on another page");
            if (reference is NullPageRecordReference)
                return null;
            if (!Headers.IsRecordFree((ushort)reference.PersistentRecordNum))
            {
                var offset = Headers.RecordShift((ushort)reference.PersistentRecordNum);
                var size = Headers.RecordSize((ushort)reference.PersistentRecordNum);
                var type = Headers.RecordType((ushort)reference.PersistentRecordNum);
                var bytes = Accessor.GetByteArray(offset, size);
                var r = new TRecordType()
                {
                    Reference = reference
                };
                var config = _config.RecordMap[type];
                config.FillFromBytes(bytes, r);
                return r;
            }
            return null;
        }

        public override void StoreRecord(TRecordType record)
        {
            if (record.Reference.Page != Reference)
                throw new ArgumentException();
            if (record.Reference is NullPageRecordReference)
                throw new ArgumentException();
            var mapKey = _config.GetRecordType(record);
            var config = _config.RecordMap[mapKey];
            if (Headers.RecordSize((ushort)record.Reference.PersistentRecordNum) < config.GetSize(record))
                throw new ArgumentException("Record size is more, than slot space available");
            SetRecord((ushort)record.Reference.PersistentRecordNum, record, config);
        }

        public override IEnumerable<PageRecordReference> IterateRecords()
        {
            throw new NotImplementedException();
        }


        ~ComplexRecordTypePage()
        {
            Dispose(true);
        }
    }
}
