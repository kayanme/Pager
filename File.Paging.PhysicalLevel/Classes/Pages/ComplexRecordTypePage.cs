﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class ComplexRecordTypePage<TRecordType> : TypedPageBase,  IPage<TRecordType> where TRecordType : TypedRecord, new()
    {
        private readonly int _pageSize;
        private readonly VariableRecordTypePageConfiguration<TRecordType> _config;
        internal ComplexRecordTypePage(IPageHeaders headers, IPageAccessor accessor, 
            PageReference reference, int pageSize,byte pageType, VariableRecordTypePageConfiguration<TRecordType> config):
            base(headers,accessor,reference,pageType)
        {
            _pageSize = pageSize;
            _config = config;
        }

        public IEnumerable<TRecordType> IterateRecords()
        {
            foreach (var i in Headers.NonFreeRecords())
            {
                var t = GetRecord(new PageRecordReference { LogicalRecordNum = i, Page = Reference });
                if (t != null)
                    yield return t;

            }
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
            

        public bool AddRecord(TRecordType type)
        {
           
            var mapKey = _config.GetRecordType(type);
            var config = _config.RecordMap[mapKey];
            var record = Headers.TakeNewRecord(mapKey, (ushort)config.GetSize(type));
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

            if (!Headers.IsRecordFree((ushort)reference.LogicalRecordNum))
            {
                var offset = Headers.RecordShift((ushort)reference.LogicalRecordNum);
                var size = Headers.RecordSize((ushort)reference.LogicalRecordNum);
                var type = Headers.RecordType((ushort)reference.LogicalRecordNum);
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

        public void StoreRecord(TRecordType record)
        {
            if (record.Reference.Page != Reference)
                throw new ArgumentException();
            if (record.Reference.LogicalRecordNum == -1)
                throw new ArgumentException();
            var mapKey = _config.GetRecordType(record);
            var config = _config.RecordMap[mapKey];
            if (Headers.RecordSize((ushort)record.Reference.LogicalRecordNum) < config.GetSize(record))
                throw new ArgumentException("Record size is more, than slot space available");
            SetRecord((ushort)record.Reference.LogicalRecordNum, record, config);
        }

        public void FreeRecord(TRecordType record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (record.Reference.LogicalRecordNum == -1)
                throw new ArgumentException("Trying to delete deleted record");
            Headers.FreeRecord((ushort)record.Reference.LogicalRecordNum);
            record.Reference.LogicalRecordNum = -1;
        }      

        public override double PageFullness => (double)Headers.TotalUsedSize / _pageSize;                           
        
        ~ComplexRecordTypePage()
        {
            Dispose(true);
        }
    }
}
