using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Exceptions;
using System.Linq;

namespace System.IO.Paging.PhysicalLevel.MemoryStubs
{
    internal sealed class PageStub<TRecord> : IPage<TRecord>,IPageInfo where TRecord:struct
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
        public int ExtentNumber { get; }

        public PageReference Reference { get; }

        public byte RegisteredPageType { get; }

        public TypedRecord<TRecord> AddRecord(TRecord record)
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
                    c.RecordMap.FillBytes(ref record, d);
                }
                if (_config is VariableRecordTypePageConfiguration<TRecord>)
                {
                    var c = _config as VariableRecordTypePageConfiguration<TRecord>;
                 
                    size = c.RecordMap.GetSize(record);
                    d = new byte[size];
                    c.RecordMap.FillBytes(ref record, d);
                }
                if (_recordSize.Values.Sum() + size >_pageSize)
                {
                    return null;
                }
                for (ushort i = 0; i < ushort.MaxValue; i++)
                {
                    var r = new LogicalPositionPersistentPageRecordReference(Reference,  i );
                  
                    if (!_records.ContainsKey(r))
                    {
                        _records.Add(r, d);
                        _recordSize.Add(r,size);
                        _recordType.Add(r, type);
                        return new TypedRecord<TRecord>{Reference = r,Data = record};
                    }
                }
                return null;
            }
        }

        public IBinarySearcher<TRecord> BinarySearch()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {           
        }

        public void FreeRecord(TypedRecord<TRecord> record)
        {
            lock (_records)
            {
                if (_records.ContainsKey(record.Reference))
                {
                    _records.Remove(record.Reference);
                }
            }
        }


        public TypedRecord<TRecord> GetRecord(PageRecordReference reference)
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

        private TypedRecord<TRecord> Retrieve(PageRecordReference reference)
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
                c.RecordMap.FillFromBytes(record, ref nr);
            }
            if (_config is VariableRecordTypePageConfiguration<TRecord>)
            {
                var c = _config as VariableRecordTypePageConfiguration<TRecord>;
                var type = _recordType[reference];
                size = _recordSize[reference];
                d = new byte[size];
                c.RecordMap.FillFromBytes(record,ref nr);
            }
            return new TypedRecord<TRecord>{Reference = reference,Data = nr};
        }

        public void StoreRecord(TypedRecord<TRecord> record)
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
                        c.RecordMap.FillBytes(ref record.Data, d);
                    }
                    if (_config is VariableRecordTypePageConfiguration<TRecord>)
                    {
                        var c = _config as VariableRecordTypePageConfiguration<TRecord>;
                        type = _recordType[record.Reference];
                        size = _recordSize[record.Reference];
                        d = new byte[size];
                        c.RecordMap.FillBytes(ref record.Data, d);
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

        public IEnumerable<TypedRecord<TRecord>> GetRecordRange(PageRecordReference start, PageRecordReference end)
        {
            lock (_records)
                return _records.SkipWhile(k => k.Key.PersistentRecordNum < start.PersistentRecordNum)
                    .TakeWhile(k => k.Key.PersistentRecordNum <= end.PersistentRecordNum)
                    .Select(k => Retrieve(k.Key));

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

        public IEnumerable<TypedRecord<TRecord>> IterateRecords()
        {
            lock (_records)
                return _records.Select(k=>GetRecord(k.Key)).ToArray();
        }


        #endregion

    }
}
