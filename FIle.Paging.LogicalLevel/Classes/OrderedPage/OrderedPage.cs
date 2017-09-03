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
        private readonly ILogicalRecordOrderManipulation _manipulation;
        private readonly Func<TRecord, TKey> _keySelector;
   

        private volatile bool _inUnsortedState;
        internal OrderedPage(IPage<TRecord> physicalPage, ILogicalRecordOrderManipulation manipulation, Func<TRecord, TKey> keySelector)
        {
            _physicalPage = physicalPage;
            _manipulation = manipulation;

            _keySelector = keySelector;
            Initialize();
        }

        private void Initialize()
        {

           

        }
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
                       
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
                _manipulation.DropOrder(record.Reference);
                _inUnsortedState = true;
            }
            finally { _lock.ExitReadLock(); }
        }

        public void Dispose()

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
                    _manipulation.ApplyOrder(records);
                }
            }
            finally
            {
                _inUnsortedState = false;
                _lock.ExitWriteLock();
            }
            _manipulation.Dispose();
            _physicalPage.Dispose();
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
                    _manipulation.ApplyOrder(records);
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

        public void Flush()
        {
            
        }
    }
}
