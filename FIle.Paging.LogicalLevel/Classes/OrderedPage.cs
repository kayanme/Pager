using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FIle.Paging.LogicalLevel.Contracts;
using Pager;
using Pager.Classes;

namespace FIle.Paging.LogicalLevel.Classes
{
    internal sealed class OrderedPage<TRecord, TKey> : IOrderedPage<TRecord> where TRecord : TypedRecord, new() where TKey : IComparable<TKey>
    {
        internal class OrderedKey : PageRecordReference
        {
            internal Guid OrderKey;
        }


        private IPage<TRecord> _physicalPage;
        private Func<TRecord, TKey> _keySelector;
        private List<TKey> _keys = new List<TKey>();
        private List<Guid> _references = new List<Guid>();
        private Dictionary<Guid, int> _backwardReferences = new Dictionary<Guid, int>();
        
        internal OrderedPage(IPage<TRecord> physicalPage, Func<TRecord, TKey> keySelector)
        {
            _physicalPage = physicalPage;
            _keySelector = keySelector;
            Initialize();
        }

        private void Initialize()
        {
            foreach(var t in _physicalPage.IterateRecords())
            {
                _keys.Add(_keySelector(t));
                _references.Add(Guid.NewGuid());
                _backwardReferences.Add(_references.Last(), _references.Count() - 1);
            }
        }

        private Stack<TRecord> _sortingInsertQueue = new Stack<TRecord>();
        private Stack<TRecord> _sortingUpdateQueue = new Stack<TRecord>();
        private Stack<TRecord> _sortingDeleteQueue = new Stack<TRecord>();

        
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public PageReference Reference => _physicalPage.Reference;

        public double PageFullness => _physicalPage.PageFullness;

        public byte RegisteredPageType => _physicalPage.RegisteredPageType;

        private int FindLogicalRecordByOrderNum(Guid orderNum) =>_backwardReferences.ContainsKey(orderNum)?_backwardReferences[orderNum]:-1;

        private enum ShiftDirection { Back,Forth}
        private struct ShiftOperation
        {
            public ShiftDirection ShiftDirection;
            public int StartPosition;
        }

        private struct KeyWrap:IComparable<KeyWrap>,IComparable
        {
            public TKey Key;
            public bool IsMax;
            public TRecord Original;
            public int CompareTo(KeyWrap other)
            {
                if (IsMax && !other.IsMax)
                    return 1;
                if (!IsMax && other.IsMax)
                    return -1;
                if (IsMax && other.IsMax)
                    return 0;
                return Key.CompareTo(other.Key);
            }

            public int CompareTo(object obj)
            {
                return CompareTo((KeyWrap)obj);
            }
        }

        private void ImplementMergeSort(TRecord[] inserts, TRecord[] updates, TRecord[] deletes)
        {
            
            var temp = _keys.Select(k => new KeyWrap { Key = k }).ToList();
            var temp2 = _references.ToList();
            foreach (var toDelete in deletes)
            {
                var num = FindLogicalRecordByOrderNum((toDelete.Reference as OrderedKey).OrderKey);
                temp[num] = new KeyWrap { IsMax = true,Original = toDelete };                                               
            }
            
            
            foreach (var toUpdate in updates)
            {
                var logicalRecordNum = FindLogicalRecordByOrderNum((toUpdate.Reference as OrderedKey).OrderKey);                
                _physicalPage.StoreRecord(toUpdate);
                temp[logicalRecordNum] = new KeyWrap { Key = _keySelector(toUpdate) };
            }
            foreach(var toInsert in inserts.OrderBy(k=>k.Reference.LogicalRecordNum))
            {
                temp.Add(new KeyWrap { Key = _keySelector(toInsert) });
                var newKey = new OrderedKey { Page = Reference, LogicalRecordNum = toInsert.Reference.LogicalRecordNum, OrderKey = Guid.NewGuid() };
                temp2.Add(newKey.OrderKey);
                toInsert.Reference = newKey;
                
            }
           
            var permutation = temp.Cast<IComparable>().Select((k, i) => new { k, i }).OrderBy(k => k.k).Select(k => k.i).ToArray();
            _keys = new List<TKey>();
            _references = new List<Guid>();
            _backwardReferences = new Dictionary<Guid, int>();
            for(var i =0;i<permutation.Length;i++)
            {

                if (temp[permutation[i]].IsMax)
                {
                    if (permutation[i] < i)
                    {
                        var r1 = new PageRecordReference { Page = Reference, LogicalRecordNum = i };
                        var r2 = new PageRecordReference { Page = Reference, LogicalRecordNum = permutation[i] };
                        _physicalPage.SwapRecords(r1,r2 );
                        temp[permutation[i]].Original.Reference = r1;
                    }                    
                }
                else
                {
                    _keys.Add(temp[permutation[i]].Key);
                    
                    _references.Add(temp2[permutation[i]]);
                    _backwardReferences.Add(temp2[permutation[i]], _references.Count - 1);
                    if (permutation[i] < i)
                        _physicalPage.SwapRecords(new PageRecordReference { Page = Reference, LogicalRecordNum = i }, new PageRecordReference { Page = Reference, LogicalRecordNum = permutation[i] });
                }
                
            }
            foreach(var t in temp.Where(k=>k.Original != null))
                _physicalPage.FreeRecord(t.Original);
        }

        public bool AddRecord(TRecord type)
        {
            if (!_physicalPage.AddRecord(type))
                return false;
          
            _sortingInsertQueue.Push(type);
            try
            {
                _lock.EnterWriteLock();
                CollectAndProcessData();                
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void CollectAndProcessData()
        {
            var inserts = Interlocked.Exchange(ref _sortingInsertQueue, new Stack<TRecord>()).ToArray();
            var updates = Interlocked.Exchange(ref _sortingUpdateQueue, new Stack<TRecord>()).ToArray();
            var deletes = Interlocked.Exchange(ref _sortingDeleteQueue, new Stack<TRecord>()).ToArray();
            ImplementMergeSort(inserts, updates, deletes);
        }

        public void FreeRecord(TRecord record)
        {
            if (!(record.Reference is OrderedKey))
                throw new InvalidOperationException("Record is from another page");
            if (record.Reference.LogicalRecordNum == -1)
                throw new ArgumentException("Record already deleted");
            _sortingDeleteQueue.Push(record);
            try
            {
                _lock.EnterWriteLock();
                CollectAndProcessData();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private PageRecordReference GetInternalReference(PageRecordReference extRef)
        {
            var r = extRef as OrderedKey;
            if (r == null) throw new InvalidOperationException("Extrenal record references are unsenseable with ordered pages");

            var rKey = FindLogicalRecordByOrderNum(r.OrderKey);
            if (rKey == -1) return null;
            var rf = new OrderedKey { Page = extRef.Page, LogicalRecordNum = rKey , OrderKey = r.OrderKey};
            return rf;
        }

        public TRecord GetRecord(PageRecordReference extRef)
        {
            try
            {
                _lock.EnterReadLock();
                var rf = GetInternalReference(extRef);
                var record = _physicalPage.GetRecord(rf);
                record.Reference = rf;
                return record;
            }
            finally { _lock.ExitReadLock(); }            
        }

        public TRecord TestGetRecord(PageRecordReference reference)
        {
            try
            {
                _lock.EnterReadLock();
                var rf =new OrderedKey { Page = reference.Page,LogicalRecordNum = reference.LogicalRecordNum , OrderKey = _references.ToArray()[reference.LogicalRecordNum]};
                var record = _physicalPage.GetRecord(rf);
                record.Reference = rf;
                return record;
            }
            finally { _lock.ExitReadLock(); }
        }

        public void StoreRecord(TRecord record)
        {
            if (!(record.Reference is OrderedKey))
                throw new InvalidOperationException("Record is from another page");
            _sortingUpdateQueue.Push(record);
            try
            {
                _lock.EnterWriteLock();
                CollectAndProcessData();                
            }
            finally { _lock.ExitWriteLock(); }
        }
       
        public void Flush()
        {
            _physicalPage.Flush();
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
                var key = new OrderedKey { Page = Reference, LogicalRecordNum = 0, OrderKey = _references.First() };
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
                var key = new OrderedKey { Page = Reference, LogicalRecordNum = _references.Count-1, OrderKey = _references.Last() };
                var rec = _physicalPage.GetRecord(key);
                rec.Reference = key;
                return rec;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SwapRecords(PageRecordReference record1, PageRecordReference record2)
        {
            throw new InvalidOperationException();
        }

        public IEnumerable<TRecord> IterateRecords()
        {
            try
            {
                _lock.EnterReadLock();
                foreach(var r in _physicalPage.IterateRecords())
                {
                    r.Reference = new OrderedKey { OrderKey = _references[r.Reference.LogicalRecordNum], Page = r.Reference.Page, LogicalRecordNum = r.Reference.LogicalRecordNum };
                    yield return r;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
