using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TimeArchiver.Contracts
{
    internal sealed class IndexInteractor : IIndexCorrection, IIndexSearch
    {
        private readonly IPage<IndexPageRecord>[] _indexes;
        
        private readonly PageRecordReference _root;

        private IPage<IndexPageRecord> _workingPage=>_indexes[_writePath];
    
        private int[] _reads = new int[2];

        private volatile int _readPath=0;
        private volatile int _writePath = 1;

        private List<PageRecordReference> _restorationList = new List<PageRecordReference>();
        private Queue<PageRecordReference> _indexAddList = new Queue<PageRecordReference>();

        public IndexInteractor(IPage<IndexPageRecord> indexPage1, IPage<IndexPageRecord> indexPage2, PageRecordReference root)
        {         
            _indexes = new[] { indexPage1, indexPage2 };
            _root = root;
        }

        public IndexInteractor(IPage<IndexPageRecord> indexPage1, IPage<IndexPageRecord> indexPage2)
        {
            _indexes = new[] { indexPage1, indexPage2 };
        
           
        }

        public PageRecordReference InitializeRoot()
        {
            var rec = _workingPage.AddRecord(new IndexPageRecord());
            _indexAddList.Enqueue(rec.Reference);
            return rec.Reference;
        }

        public void CreateDataBlock(DataPageRef records)
        {
            var rec = _workingPage.GetRecord(_root);
            rec.Data.Start = records.Start;
            rec.Data.End = records.End;
            rec.Data.MaxUnderlyingDepth = 0;
            rec.Data.Data = records.DataReference;
            rec.Data.ChildrenTwo = null;
            _workingPage.StoreRecord(rec);
            _restorationList.Add(rec.Reference);

        }

        public IndexRecord CreateDataBlock(IndexRecord root, DataPageRef records)
        {

            var rec = _workingPage.GetRecord(root._recordNum);
            if (root.MaxUnderlyingDepth == 0)
            {
                rec = CreateUnderlayingIndexRecord(rec);
            }
            var dataRec = new IndexPageRecord
            {
                Start = records.Start,
                End = records.End,
                MaxUnderlyingDepth = 0,
                Data = records.DataReference,
                ChildrenOne = null,
                ChildrenTwo = null
            };
            var index = _workingPage.AddRecord(dataRec);
            _indexAddList.Enqueue(index.Reference);
            rec.Data.ChildrenTwo = index.Reference;
            _workingPage.StoreRecord(rec);
            _restorationList.Add(rec.Reference);
            return new IndexRecord
            {
                Start = rec.Data.Start,
                End = rec.Data.End,
                MaxUnderlyingDepth = 1,
                StoresData = false,
                _recordNum = rec.Reference,
                _parentNum = root._parentNum
            };

        }

        private TypedRecord<IndexPageRecord> CreateUnderlayingIndexRecord(TypedRecord<IndexPageRecord> record)
        {
            var downedRec = new IndexPageRecord
            {
                Start = record.Data.Start,
                End = record.Data.End,
                MaxUnderlyingDepth = record.Data.MaxUnderlyingDepth,
                ChildrenOne = record.Data.ChildrenOne,
                ChildrenTwo = record.Data.ChildrenTwo,
                Data = record.Data.Data
            };
            var savedRecord = _workingPage.AddRecord(downedRec);
            _indexAddList.Enqueue(savedRecord.Reference);
            record.Data.Data = null;
            record.Data.ChildrenOne = savedRecord.Reference;
            record.Data.ChildrenTwo = null;
            record.Data.MaxUnderlyingDepth = (short)(record.Data.MaxUnderlyingDepth + 1);
         
            return record;

        }      

        public IndexRecord[] GetChildren(IndexRecord parent)
        {
            return GetChildren(_workingPage, parent);
        }

        private IndexRecord[] GetChildren(IPage<IndexPageRecord> page, IndexRecord parent)
        {

            var root = page.GetRecord(parent._recordNum);
            if (root.Data.MaxUnderlyingDepth == 0)
                return new IndexRecord[0];
            var childOne = page.GetRecord(root.Data.ChildrenOne);
            var childTwo = page.GetRecord(root.Data.ChildrenTwo);
            return new[]
            {
                    FromFileRecord(childOne,root.Reference),
                    FromFileRecord(childTwo,root.Reference)
                };

        }

        IndexRecord[] IIndexSearch.GetChildren(IndexRecord parent,IDisposable readToken)
        {
            return GetChildren(_indexes[((P)readToken).Path], parent);
        }

        private IndexRecord FromFileRecord(TypedRecord<IndexPageRecord> fileRecord, PageRecordReference parent) =>
            new IndexRecord
            {
                Start = fileRecord.Data.Start,
                End = fileRecord.Data.End,
                MaxUnderlyingDepth = fileRecord.Data.MaxUnderlyingDepth,
                StoresData = fileRecord.Data.MaxUnderlyingDepth == 0,
                _recordNum = fileRecord.Reference,
                _parentNum = parent
            };

        public DataPageRef GetDataRef(IndexRecord record, IDisposable readToken)
        {

            var rec = _indexes[((P)readToken).Path].GetRecord(record._recordNum);
            if (rec.Data.MaxUnderlyingDepth != 0)
                throw new ArgumentException("record is not a data reference");
            return new DataPageRef { Start = rec.Data.Start, End = rec.Data.End, DataReference = rec.Data.Data };

        }

        public IndexRecord? GetRoot()
        {

            var rootRec = _workingPage.GetRecord(_root);
            if (rootRec.Data.Data == null && rootRec.Data.ChildrenOne == null)
                return null;
            return FromFileRecord(rootRec, null);

        }

        IndexRecord? IIndexSearch.GetRoot(IDisposable readToken)
        {

            var rootRec = _indexes[((P)readToken).Path].GetRecord(_root);
            if
                (rootRec.Data.Data == null && rootRec.Data.ChildrenOne == null)
                return null;
            return FromFileRecord(rootRec, null);

        }

        public IndexRecord MoveIndex(IndexRecord newRoot, IndexRecord recordToMove)
        {

            var rootRec = _workingPage.GetRecord(newRoot._recordNum);
            rootRec = CreateUnderlayingIndexRecord(rootRec);
            var grandParent = _workingPage.GetRecord(newRoot._parentNum);
            var movRec = _workingPage.GetRecord(recordToMove._recordNum);
            var movParent = _workingPage.GetRecord(recordToMove._parentNum);
            var notMovedRecord = movParent.Data.ChildrenOne == movRec.Reference ? movParent.Data.ChildrenTwo : movParent.Data.ChildrenOne;
            if (grandParent.Data.ChildrenOne == rootRec.Reference)
            {
                Debug.Assert(grandParent.Data.ChildrenTwo == movParent.Reference);
                grandParent.Data.ChildrenTwo = notMovedRecord;
            }
            else
            {
                Debug.Assert(grandParent.Data.ChildrenOne == movParent.Reference);
                grandParent.Data.ChildrenOne = notMovedRecord;
            }
            rootRec.Data.ChildrenTwo = movRec.Reference;
            rootRec.Data.MaxUnderlyingDepth = (short)Math.Max(rootRec.Data.MaxUnderlyingDepth, movRec.Data.MaxUnderlyingDepth + 1);
            grandParent.Data.MaxUnderlyingDepth = (short)Math.Max(rootRec.Data.MaxUnderlyingDepth + 1, _workingPage.GetRecord(notMovedRecord).Data.MaxUnderlyingDepth + 1);
            _workingPage.StoreRecord(grandParent);
            _workingPage.StoreRecord(rootRec);
            _restorationList.Add(grandParent.Reference);
            _restorationList.Add(rootRec.Reference);
            return FromFileRecord(rootRec, grandParent.Reference);


        }

        public async Task PrepareIndexChange()
        {
            return;
        }

        public IndexRecord ResetTreeDepth(IndexRecord record)
        {
            var rootRec = _workingPage.GetRecord(record._recordNum);
            if (rootRec.Data.MaxUnderlyingDepth == 0)
                return record;
            rootRec.Data.MaxUnderlyingDepth = (short)(Math.Max(_workingPage.GetRecord(rootRec.Data.ChildrenOne).Data.MaxUnderlyingDepth, _workingPage.GetRecord(rootRec.Data.ChildrenTwo).Data.MaxUnderlyingDepth) + 1);
            _workingPage.StoreRecord(rootRec);
            _restorationList.Add(rootRec.Reference);
            return FromFileRecord(rootRec, record._parentNum);

        }
        public IndexRecord ResizeIndex(IndexRecord record, long start, long end)
        {
            var rootRec = _workingPage.GetRecord(record._recordNum);
            rootRec.Data.Start = start;
            rootRec.Data.End = end;
            _workingPage.StoreRecord(rootRec);
            _restorationList.Add(rootRec.Reference);
            return FromFileRecord(rootRec, record._parentNum);
        }

        private struct P : IDisposable
        {
            public int Path { get; }
            private readonly IndexInteractor ii;

            public P(int i,IndexInteractor ii)
            {
                this.Path = i;
                this.ii = ii;
                Interlocked.Increment(ref ii._reads[i]);
            }
            public void Dispose()
            {
                if (Interlocked.Decrement(ref ii._reads[Path]) == 1)
                {

                }
            }
        }

        public IDisposable ReadBlock()
        {           
            return new P(_readPath,this);
        }


        public async Task FinalizeIndexChange()
        {
            _workingPage.Flush();
            _writePath = Interlocked.Exchange(ref _readPath, _writePath);
            while (_reads[_writePath] >0) await Task.Delay(10);
            foreach (var addedIndex in _indexAddList)
            {
                var sourceRec = _indexes[_readPath].GetRecord(addedIndex);
                if (_workingPage.AddRecord(sourceRec.Data) == null) Debug.Fail("Unknown index fail");
            }
            foreach (var changedIndex in _restorationList)
            {
                var sourceRec = _indexes[_readPath].GetRecord(changedIndex);
                var recToRestore = _workingPage.GetRecord(changedIndex);
                recToRestore.Data = sourceRec.Data;
                _workingPage.StoreRecord(recToRestore);
            }            
            _workingPage.Flush();
            _restorationList.Clear();
            _indexAddList.Clear();
        }
    }
}
