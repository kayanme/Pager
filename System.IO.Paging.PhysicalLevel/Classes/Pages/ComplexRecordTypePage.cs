using System;
using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Linq;
using System.Threading;


namespace System.IO.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class ComplexRecordTypePage<TRecordType> : TypedPageBase, IPage<TRecordType> where TRecordType : struct
    {
        internal IPageHeaders Headers;
        internal IPageAccessor Accessor;
        private readonly VariableRecordTypePageConfiguration<TRecordType> _config;
        internal ComplexRecordTypePage(IPageHeaders headers, IPageAccessor accessor,
            PageReference reference, int pageSize, byte pageType, VariableRecordTypePageConfiguration<TRecordType> config, Action actionToClean) :
            base(reference, actionToClean)
        {
            Headers = headers;
            Accessor = accessor;
            _config = config;
        }



        private void SetRecord(ushort offset, TRecordType record, RecordDeclaration<TRecordType> config) 
        {
            var recordStart = Headers.RecordShift(offset);
            var recordSize = Headers.RecordSize(offset);
            
            var bytes = Accessor.GetByteArray(recordStart, recordSize);
            config.FillBytes(ref record, bytes);

            Thread.BeginCriticalRegion();
            Headers.SetNewRecordInfo(offset, recordSize);
            Accessor.SetByteArray(bytes, recordStart, recordSize);
            Thread.EndCriticalRegion();
        }


        public TypedRecord<TRecordType> AddRecord(TRecordType type)
        {

          //  var mapKey = _config.GetRecordType(type);
            //var config = _config.RecordMap[0];
            var record = Headers.TakeNewRecord((ushort)_config.RecordMap.GetSize(type));
            if (record == -1)
                return null;
            SetRecord((ushort)record, type, _config.RecordMap);
            var typed = new TypedRecord<TRecordType> { Data = type };
            if (typed.Reference == null)
                typed.Reference = _config.WithLogicalSort ?
                    (PageRecordReference)new RowKeyPersistentPageRecordReference(Reference, 0)
                                       : new LogicalPositionPersistentPageRecordReference(Reference, (ushort)record);

            return typed;
        }

        public TypedRecord<TRecordType> GetRecord(PageRecordReference reference)
        {
            if (Reference != reference.Page)
                throw new ArgumentException("The record is on another page");
            if (reference is NullPageRecordReference)
                return null;
            if (!Headers.IsRecordFree((ushort)reference.PersistentRecordNum))
            {
                var offset = Headers.RecordShift((ushort)reference.PersistentRecordNum);
                var size = Headers.RecordSize((ushort)reference.PersistentRecordNum);                
                var bytes = Accessor.GetByteArray(offset, size);
                var r = new TypedRecord<TRecordType>()
                {
                    Reference = reference
                };

                var config = _config.RecordMap;
                config.FillFromBytes(bytes, ref r.Data);
                return r;
            }
            return null;
        }

        public void StoreRecord(TypedRecord<TRecordType> record)
        {
            if (record.Reference.Page != Reference)
                throw new ArgumentException();
            if (record.Reference is NullPageRecordReference)
                throw new ArgumentException();
            
            var config = _config.RecordMap;
            if (Headers.RecordSize((ushort)record.Reference.PersistentRecordNum) < config.GetSize(record.Data))
                throw new ArgumentException("Record size is more, than slot space available");
            SetRecord((ushort)record.Reference.PersistentRecordNum, record.Data, config);
        }

        public IEnumerable<TypedRecord<TRecordType>> IterateRecords()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            Accessor.Flush();
        }

        public void FreeRecord(TypedRecord<TRecordType> record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (record.Reference is NullPageRecordReference)
                throw new ArgumentException("Trying to delete deleted record");


            Headers.FreeRecord((ushort)record.Reference.PersistentRecordNum);
            record.Reference = new NullPageRecordReference(Reference);
        }

        public IEnumerable<TypedRecord<TRecordType>> GetRecordRange(PageRecordReference start, PageRecordReference end)
        {
            throw new NotImplementedException();
        }

        ~ComplexRecordTypePage()
        {
            Dispose(true);
        }
    }
}
