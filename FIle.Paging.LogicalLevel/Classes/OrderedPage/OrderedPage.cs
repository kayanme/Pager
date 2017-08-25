using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes
{
    internal sealed class OrderedPage<TRecord, TKey> : IOrderedPage<TRecord> where TRecord : TypedRecord, new() where TKey : IComparable<TKey>
    {      


        private readonly IPage<TRecord> _physicalPage;
        private readonly Func<TRecord, TKey> _keySelector;
   

        private volatile bool _inUnsortedState;
        internal OrderedPage(IPage<TRecord> physicalPage, Func<TRecord, TKey> keySelector)
        {
            _physicalPage = physicalPage;
            Debug.Assert(physicalPage is IPhysicalRecordManipulation, "physicalPage is IPhysicalLevelManipulation");
            _keySelector = keySelector;
            Initialize();
        }

        private void Initialize()
        {

            var records =
                _physicalPage.IterateRecords()
                    .Select(k => new {k, key = _keySelector(GetRecord(k))})
                    .OrderBy(k => k.key)
                    .Select(k => k.k)
                    .ToArray();
            (_physicalPage as ILogicalRecordOrderManipulation).ApplyOrder(records);

        }



        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public PageReference Reference => _physicalPage.Reference;

        public double PageFullness => _physicalPage.PageFullness;
        public int UsedRecords
        {
            get { return _physicalPage.UsedRecords; }
        }

        public byte RegisteredPageType => _physicalPage.RegisteredPageType;

      
     
      

        public bool AddRecord(TRecord type)
        {            

            try
            {
                _lock.EnterReadLock();
                if (!_physicalPage.AddRecord(type))
                    return false;
                _inUnsortedState = true;
              
                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void FreeRecord(TRecord record)
        {
            if (!(record.Reference is RowKeyPersistentPageRecordReference))
                throw new InvalidOperationException("Record is from another page");
            if (record.Reference is NullPageRecordReference)
                throw new ArgumentException("Record already deleted");
          
            try
            {
                _lock.EnterReadLock();
                _physicalPage.FreeRecord(record);
                _inUnsortedState = true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }



        public TRecord GetRecord(PageRecordReference extRef)
        {

            var record = _physicalPage.GetRecord(extRef);
            return record;

        }

        public TRecord TestGetRecord(PageRecordReference reference)
        {
            try
            {
                _lock.EnterReadLock();               
                var record = _physicalPage.GetRecord(reference);                
                return record;
            }
            finally { _lock.ExitReadLock(); }
        }

        public void StoreRecord(TRecord record)
        {
          
           
            try
            {
                _lock.EnterReadLock();
                _physicalPage.StoreRecord(record);
                (_physicalPage as ILogicalRecordOrderManipulation).DropOrder(record.Reference);
                _inUnsortedState = true;
            }
            finally { _lock.ExitReadLock(); }
        }
      
        public void Dispose()
        {
            _physicalPage.Dispose();
        }

      

        public TRecord First()
        {
            try
            {
                _lock.EnterReadLock();
                var key = new LogicalPositionPersistentPageRecordReference(Reference,0);
                var rec = _physicalPage.GetRecord(key);
                rec.Reference = key;
                return rec;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TRecord Last()
        {
            try
            {
                _lock.EnterReadLock();
                var key = new LogicalPositionPersistentPageRecordReference(Reference,  (ushort)(_physicalPage.UsedRecords-1));
                var rec = _physicalPage.GetRecord(key);
                rec.Reference = key;
                return rec;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

     

        public IEnumerable<PageRecordReference> IterateRecords()
        {
            try
            {
                _lock.EnterWriteLock();
                if (_inUnsortedState)
                {
                    var records =
                    _physicalPage.IterateRecords()
                        .Select(k => new {k, key = _keySelector(GetRecord(k))})
                        .OrderBy(k => k.key)
                        .Select(k => k.k)
                        .ToArray();
                    (_physicalPage as ILogicalRecordOrderManipulation).ApplyOrder(records);
                    return records;
                }
                return _physicalPage.IterateRecords();
            }
            finally
            {
                _inUnsortedState = false;
                _lock.ExitWriteLock();
            }
        }
    }
}
