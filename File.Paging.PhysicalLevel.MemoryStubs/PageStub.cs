﻿using System;
using System.Collections.Generic;
using System.Linq;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Exceptions;

namespace File.Paging.PhysicalLevel.MemoryStubs
{
    internal sealed class PageStub<TRecord> : IPage<TRecord> where TRecord:TypedRecord,new()
    {
        private readonly Dictionary<PageRecordReference, byte[]> _records = new Dictionary<PageRecordReference, byte[]>();
        private readonly Dictionary<PageRecordReference, int> _recordSize = new Dictionary<PageRecordReference, int>();
        private readonly Dictionary<PageRecordReference, byte> _recordType = new Dictionary<PageRecordReference, byte>();
        private readonly int _pageSize;
        private readonly PageContentConfiguration _config;
        public PageStub(PageReference r,PageContentConfiguration config,int pageSize,byte pageType)
        {
            Reference = r;
            _config = config;
            _pageSize = pageSize;
            RegisteredPageType = pageType;
        }      

        public double PageFullness => throw new NotImplementedException();
        public int UsedRecords => _records.Count;

        public PageReference Reference { get; }

        public byte RegisteredPageType { get; }

        public bool AddRecord(TRecord record)
        {
            lock (_records)
            {
                int size = 0;
                byte type = 1;
                byte[] d = null;
                if (_config is FixedRecordTypePageConfiguration<TRecord>)
                {
                    var c = _config as FixedRecordTypePageConfiguration<TRecord>;
                    size = c.RecordMap.GetSize;
                    d = new byte[size];
                    c.RecordMap.FillBytes(record, d);
                }
                if (_config is VariableRecordTypePageConfiguration<TRecord>)
                {
                    var c = _config as VariableRecordTypePageConfiguration<TRecord>;
                    type = c.GetRecordType(record);
                    size = c.RecordMap[type].GetSize(record);
                    d = new byte[size];
                    c.RecordMap[type].FillBytes(record, d);
                }
                if (_recordSize.Values.Sum() + size >_pageSize)
                {
                    return false;
                }
                for (ushort i = 0; i < ushort.MaxValue; i++)
                {
                    var r = new LogicalPositionPersistentPageRecordReference(Reference,  i );
                  
                    if (!_records.ContainsKey(r))
                    {
                        _records.Add(r, d);
                        _recordSize.Add(r,size);
                        _recordType.Add(r, type);
                        return true;
                    }
                }
                return false;
            }
        }

        public void Flush()
        {           
        }

        public void FreeRecord(TRecord record)
        {
            lock (_records)
            {
                if (_records.ContainsKey(record.Reference))
                {
                    _records.Remove(record.Reference);
                }
            }
        }

      
        public TRecord GetRecord(PageRecordReference reference)
        {
            lock (_records)
            {
                if (_records.ContainsKey(reference))
                {
                    return Retrieve(reference);
                }
                return null;
            }
        }

        private TRecord Retrieve(PageRecordReference reference)
        {
            int size = 0;
            byte[] d = null;
            var record = _records[reference];
            var nr = new TRecord();
            if (_config is FixedRecordTypePageConfiguration<TRecord>)
            {
                var c = _config as FixedRecordTypePageConfiguration<TRecord>;
                size = c.RecordMap.GetSize;
                d = new byte[size];
                c.RecordMap.FillFromBytes(record, nr);
            }
            if (_config is VariableRecordTypePageConfiguration<TRecord>)
            {
                var c = _config as VariableRecordTypePageConfiguration<TRecord>;
                var type = _recordType[reference];
                size = _recordSize[reference];
                d = new byte[size];
                c.RecordMap[type].FillFromBytes(record, nr);
            }
            return nr;
        }

        public void StoreRecord(TRecord record)
        {
            lock (_records)
            {
                if (_records.ContainsKey(record.Reference))
                {
                    int size = 0;
                    byte type = 1;
                    byte[] d = null;
             
                
                    if (_config is FixedRecordTypePageConfiguration<TRecord>)
                    {
                        var c = _config as FixedRecordTypePageConfiguration<TRecord>;
                        size = c.RecordMap.GetSize;
                        d = new byte[size];
                        c.RecordMap.FillBytes(record, d);
                    }
                    if (_config is VariableRecordTypePageConfiguration<TRecord>)
                    {
                        var c = _config as VariableRecordTypePageConfiguration<TRecord>;
                        type = _recordType[record.Reference];
                        size = _recordSize[record.Reference];
                        d = new byte[size];
                        c.RecordMap[type].FillBytes(record, d);
                    }
                    if (_recordSize[record.Reference] > size)
                        throw new InvalidOperationException();
                    _recordSize[record.Reference] = size;
                    _recordType[record.Reference] = type;
                    _records[record.Reference] = d;
                }
                else throw new RecordWriteConflictException(); 
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                   
                }

                _disposedValue = true;
            }
        }

        
         ~PageStub()
        {            
            Dispose(false);
        }

        
        public void Dispose()
        {         
            Dispose(true);          
            GC.SuppressFinalize(this);
        }

        public void SwapRecords(PageRecordReference record1, PageRecordReference record2)
        {
            lock (_records)
            {
                var t = _records[record1];
                _records[record1] = _records[record2];
                _records[record2] = t;
            }
        }

        public IEnumerable<PageRecordReference> IterateRecords()
        {
            lock (_records)
                return _records.Keys.ToArray();
        }


        #endregion

    }
}
