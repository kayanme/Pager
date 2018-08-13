using TimeArchiver.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes;

namespace Test.TimeArchiver
{
    internal class MockIndexInteraction : IIndexCorrection,IIndexSearch
    {

        private IndexRecord? _root;
        private Dictionary<byte, (IndexRecord, IndexRecord)> _hierarchy = new Dictionary<byte, (IndexRecord, IndexRecord)>();
        private static byte _key = 1;
        public byte newKey() => _key++;

        public virtual void CreateDataBlock(DataPageRef records)
        {
            _root = new IndexRecord(records.Start, records.End, true, 0,newKey());
            
        }

        public void SetRoot(IndexRecord rec)
        {
            _root = rec;
        }

        public void AddChildren(IndexRecord r,IndexRecord[] children)
        {
            Debug.Assert(r.TestKey != 0);
            foreach (var c in children) Debug.Assert(c.TestKey != 0);
            _hierarchy.Add(r.TestKey, (children[0], children[1]));
        }

        public virtual IndexRecord CreateDataBlock(IndexRecord root, DataPageRef records)
        {
            if (root.StoresData)
                root = CreateUnderlayingIndexRecord(root);
            var db = new IndexRecord(records.Start, records.End, true, 0, newKey());
            _hierarchy[root.TestKey] = (_hierarchy[root.TestKey].Item1, db);
            return root;
        }

        private  IndexRecord CreateUnderlayingIndexRecord(IndexRecord record)
        {
            var ind = new IndexRecord(record.Start, record.End, false, (short)(record.MaxUnderlyingDepth + 1), newKey());
            if (_root.Value.TestKey == record.TestKey)
            {
                _root = ind;
            }
            else
            {
                var oldParent = _hierarchy.First(k => k.Value.Item1.TestKey == record.TestKey || k.Value.Item2.TestKey == record.TestKey).Key;
                var oldRec = _hierarchy[oldParent].Item1.TestKey == record.TestKey ? _hierarchy[oldParent].Item2 : _hierarchy[oldParent].Item1;
                _hierarchy[oldParent] = (oldRec, ind);
             
            }
            _hierarchy.Add(ind.TestKey, (record, default(IndexRecord)));
            return ind;
        }

        public async Task FinalizeIndexChange()
        {
            
        }

        public virtual IndexRecord[] GetChildren(IndexRecord parent)
        {
            var (a, b) = _hierarchy[parent.TestKey];
            return new[] { a, b };
        }

        public virtual IndexRecord? GetRoot()
        {
            return _root;
        }

        

        public virtual IndexRecord MoveIndex(IndexRecord newRoot, IndexRecord recordToMove)
        {
            if (newRoot.StoresData)
                newRoot = CreateUnderlayingIndexRecord(newRoot);
            var oldParent = _hierarchy.First(k => k.Value.Item1.TestKey == recordToMove.TestKey || k.Value.Item2.TestKey == recordToMove.TestKey).Key;
            var oldGrandParent = _hierarchy.First(k => k.Value.Item1.TestKey == oldParent || k.Value.Item2.TestKey == oldParent).Key;
            var ogph = _hierarchy[oldGrandParent];
            var oph = _hierarchy[oldParent];
            var remainedChild = ogph.Item1.TestKey == oldParent ? ogph.Item2 : ogph.Item1;
            var remainedGrandChild = oph.Item1.TestKey == recordToMove.TestKey ? oph.Item2 : oph.Item1;
            _hierarchy[oldGrandParent] = (remainedChild,remainedGrandChild);
            _hierarchy[newRoot.TestKey] = (_hierarchy[newRoot.TestKey].Item1, recordToMove);
            ResetTreeDepth(newRoot);
            ResetTreeDepth(recordToMove);
            return recordToMove;
        }

        public async Task PrepareIndexChange()
        {
         
        }

        public virtual IndexRecord ResetTreeDepth(IndexRecord record)
        {
            if (!_hierarchy.ContainsKey(record.TestKey))
                return record;
            var newDepth = Math.Max(_hierarchy[record.TestKey].Item1.MaxUnderlyingDepth, _hierarchy[record.TestKey].Item2.MaxUnderlyingDepth);
            var nr = new IndexRecord(record.Start, record.End, record.StoresData, newDepth, record.TestKey);
            if (record.TestKey == _root.Value.TestKey)
            {
                _root = nr;
            }
            else
            {
                var parent = _hierarchy.First(k => k.Value.Item1.TestKey == record.TestKey || k.Value.Item2.TestKey == record.TestKey).Key;
                _hierarchy[parent] = (_hierarchy[parent].Item1.TestKey == record.TestKey ? _hierarchy[parent].Item2 : _hierarchy[parent].Item1, nr);
            }
            return nr;
        }

        public virtual IndexRecord ResizeIndex(IndexRecord record, long start, long end)
        {
            if (_root.Value.TestKey == record.TestKey)
            {
                _root = new IndexRecord(start, end, record.StoresData, record.MaxUnderlyingDepth, record.TestKey);
                return _root.Value;
            }
            else
            {
                var parent = _hierarchy.First(k => k.Value.Item1.TestKey == record.TestKey || k.Value.Item2.TestKey == record.TestKey).Key;
                var newDepth = Math.Max(_hierarchy[record.TestKey].Item1.MaxUnderlyingDepth, _hierarchy[record.TestKey].Item2.MaxUnderlyingDepth);
                var nr = new IndexRecord(start, end, record.StoresData, record.MaxUnderlyingDepth, record.TestKey);
                _hierarchy[parent] = (_hierarchy[parent].Item1.TestKey == record.TestKey ? _hierarchy[parent].Item2 : _hierarchy[parent].Item1, nr);
                return nr;
            }
        }

        public DataPageRef GetDataRef(IndexRecord record)
        {
            throw new NotImplementedException();
        }

        public IDisposable ReadBlock()
        {
            throw new NotImplementedException();
        }

        public PageRecordReference InitializeRoot()
        {
            throw new NotImplementedException();
        }
    }
}
