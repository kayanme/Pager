using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Classes.References;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.OrderedPage;
using FIle.Paging.LogicalLevel.Contracts;

namespace FIle.Paging.LogicalLevel.Classes
{
    internal sealed class OrderedPage<TRecord, TKey> : IOrderedPage<TRecord,TKey> 
        where TRecord : struct
        where TKey : IComparable<TKey>
    {      


        private readonly IPage<TRecord> _physicalPage;
        private readonly ILogicalRecordOrderManipulation _manipulation;
        private readonly PageReference _reference;
        private readonly IPageManager _manager;
        private readonly Func<TRecord, TKey> _keySelector;
        private readonly SortStateContoller _sortController;


        internal OrderedPage(PageReference reference,
            IPageManager manager,
            Func<TRecord, TKey> keySelector,
            SortStateContoller sortController)
        {
            _physicalPage = manager.GetRecordAccessor<TRecord>(reference);
            _manipulation = manager.GetSorter<TRecord>(reference);

            _reference = reference;
            _manager = manager;
            _keySelector = keySelector;
            _sortController = sortController;

        }
        
                       
        public TypedRecord<TRecord> AddRecord(TRecord type)
        {            

            try
            {
                _sortController.AcquireReadLock();
                var r = _physicalPage.AddRecord(type);
                if (r!=null)
                   _sortController.IsSorted = false;
              
                return r;
            }
            finally
            {
                _sortController.ReleaseReadLock();
            }
        }

        public void FreeRecord(TypedRecord<TRecord> record)
        {
            if (!(record.Reference is RowKeyPersistentPageRecordReference))
                throw new InvalidOperationException("Record is from another page");
            if (record.Reference is NullPageRecordReference)
                throw new ArgumentException("Record already deleted");
          
            try
            {
                _sortController.AcquireReadLock();
                _physicalPage.FreeRecord(record);
                _sortController.IsSorted = false;
            }
            finally
            {
                _sortController.ReleaseReadLock();
            }
        }



        public TypedRecord<TRecord> GetRecord(PageRecordReference extRef)
        {

            var record = _physicalPage.GetRecord(extRef);
            return record;

        }

        public TypedRecord<TRecord> TestGetRecord(PageRecordReference reference)
        {
            try
            {
                _sortController.AcquireReadLock();   
                var record = _physicalPage.GetRecord(reference);                
                return record;
            }
            finally { _sortController.ReleaseReadLock(); }
        }

        public void StoreRecord(TypedRecord<TRecord> record)
        {
          
           
            try
            {
                _sortController.AcquireReadLock();
                _physicalPage.StoreRecord(record);
                _manipulation.DropOrder(record.Reference);
                _sortController.IsSorted = false;
            }
            finally { _sortController.ReleaseReadLock(); }
        }

        public TypedRecord<TRecord> FindByKey(TKey key)
        {
            try
            {
                _sortController.AcquireSortLock();
                ResortIfNeeded();
                using (var s = _manager.GetBinarySearchForPage<TRecord>(_reference))
                {
                    if (s.Current == null)
                        return null;
                    TKey curKey;
                    do
                    {
                        curKey = _keySelector(s.Current.Data);
                        if (curKey.CompareTo(key) <0)
                            if (!s.MoveRight())
                                return null;
                        if (curKey.CompareTo(key) >0)
                            if (!s.MoveLeft())
                                return null;
                    } while (curKey.CompareTo(key) != 0);
                    return s.Current;
                }
            }
            finally
            {
                _sortController.ReleaseSortLock();
            }
        }

        public void Dispose()

        {
            try
            {
                _sortController.AcquireSortLock();
                 ResortIfNeeded();
            }
            finally
            {
                _sortController.IsSorted = true;
                _sortController.ReleaseSortLock();
            }
            _manipulation.Dispose();
            _physicalPage.Dispose();
        }

        public IEnumerable<TypedRecord<TRecord>> IterateRecords()
        {
            try
            {
                _sortController.AcquireSortLock();
                 ResortIfNeeded();
                return _physicalPage.IterateRecords();
            }
            finally
            {
                
                _sortController.ReleaseSortLock();
            }
        }

        private void ResortIfNeeded()
        {
            if (!_sortController.IsSorted)
            {
                var records =
                    _physicalPage.IterateRecords()
                        .Select(k => new {k, key = _keySelector(k.Data)})
                        .OrderBy(k => k.key)
                        .Select(k => k.k)
                        .ToArray();
                _manipulation.ApplyOrder(records.Select(k => k.Reference).ToArray());
                _sortController.IsSorted = true;
            }
        }

        public void Flush()
        {
            
        }
    }
}
