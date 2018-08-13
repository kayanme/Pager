using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimeArchiver.Contracts;

namespace TimeArchiver.Classes
{


    internal class DataInteraction<T> : IDataInteraction<T> where T : struct
    {
        private readonly IIndexCorrection _indexInteraction;
        private readonly IIndexSearch _indexSearch;
        private readonly IDataPageInteractor<T> _pageInteractor;

        public DataInteraction(IIndexCorrection indexInteraction, IIndexSearch indexSearch, IDataPageInteractor<T> pageInteractor)
        {
            this._indexInteraction = indexInteraction ?? throw new ArgumentNullException(nameof(indexInteraction));
            this._indexSearch = indexSearch;
            this._pageInteractor = pageInteractor ?? throw new ArgumentNullException(nameof(pageInteractor));
        }

        private (long,long) GetUnitedBorders((long ,long ) block1,(long ,long ) block2)
            => (Math.Min(block1.Item1, block2.Item1), Math.Max(block1.Item2, block2.Item2));

        private (long, long) GetUnitedBorders(IndexRecord block1, (long, long) block2)
         => (Math.Min(block1.Start, block2.Item1), Math.Max(block1.End, block2.Item2));

        private (long, long) GetUnitedBorders(IndexRecord block1, IndexRecord block2)
        => (Math.Min(block1.Start, block2.Start), Math.Max(block1.End, block2.End));


        private long Range((long , long) b) => b.Item2-b.Item1;
        private long Range(IndexRecord b) => b.End - b.Start;

        private IndexRecord SelectIndexToInsert((long,long) bounds,IndexRecord index1,IndexRecord index2)
        {
            var diff1 = Range(GetUnitedBorders(index1,bounds)) - Range(index1);
            var diff2 = Range(GetUnitedBorders(index2, bounds))-Range(index2);
            if (diff2<diff1)
                return index2;
            return index1;
        }

        private IndexRecord ProcessIndexLevel(IndexRecord currentRoot, DataPageRef page)
        {
            
            var (lowbound, upbound) = (page.Start, page.End);
                    
            var firstLevelChildren = _indexInteraction.GetChildren(currentRoot);
            Debug.Assert(firstLevelChildren.Length == 2, "firstLevelChildren.Length == 2");
            var indexToInsertBlock = SelectIndexToInsert((lowbound, upbound), firstLevelChildren[0], firstLevelChildren[1]);
            var otherIndex = firstLevelChildren[0].Equals(indexToInsertBlock) ? firstLevelChildren[1] : firstLevelChildren[0];
            
            indexToInsertBlock = InsertOrProcessNextlevel(page, lowbound, upbound, indexToInsertBlock);
            var secondLevelChildren = _indexInteraction.GetChildren(indexToInsertBlock);
            if (secondLevelChildren[0].MaxUnderlyingDepth != secondLevelChildren[1].MaxUnderlyingDepth)
            {
                var lesserGrandChild = secondLevelChildren[0].MaxUnderlyingDepth > secondLevelChildren[1].MaxUnderlyingDepth ? secondLevelChildren[1] : secondLevelChildren[0];
                
                _indexInteraction.MoveIndex(otherIndex, lesserGrandChild);
                var (ln2, un2) = GetUnitedBorders(otherIndex, (lesserGrandChild.Start, lesserGrandChild.End));
                otherIndex = _indexInteraction.ResizeIndex(otherIndex, ln2, un2);
                otherIndex = _indexInteraction.ResetTreeDepth(otherIndex);
            }
            var (ln, un) = GetUnitedBorders(indexToInsertBlock, otherIndex);
            currentRoot = _indexInteraction.ResizeIndex(currentRoot, ln, un);
            currentRoot = _indexInteraction.ResetTreeDepth(currentRoot);
            return currentRoot;
        }

        private IndexRecord InsertOrProcessNextlevel(DataPageRef page, long lowbound, long upbound, IndexRecord indexToInsertBlock)
        {
            var (newLowBorder, newUpBorder) = GetUnitedBorders(indexToInsertBlock, (lowbound, upbound));
            if (indexToInsertBlock.StoresData || lowbound <= indexToInsertBlock.Start && upbound >= indexToInsertBlock.End)
            {
                indexToInsertBlock = _indexInteraction.CreateDataBlock(indexToInsertBlock, page);
                indexToInsertBlock = _indexInteraction.ResizeIndex(indexToInsertBlock, newLowBorder, newUpBorder);
            }
            else
            {
                indexToInsertBlock= ProcessIndexLevel(indexToInsertBlock, page);
            }

            return indexToInsertBlock;
        }

        public async Task AddBlock(DataRecord<T>[] sortedData)
        {
            var (lowbound, upbound) = (sortedData.First().Stamp, sortedData.Last().Stamp);
            var root = _indexInteraction.GetRoot();
            var page = _pageInteractor.CreateDataBlock(sortedData);
            if (root == null)
            {                
                _indexInteraction.CreateDataBlock(page);
                return;
            }
            await _indexInteraction.PrepareIndexChange();
            var currentIndex = root.Value;
            if (currentIndex.StoresData)
            {
                currentIndex = _indexInteraction.CreateDataBlock(currentIndex, page);
                var (a, b) = GetUnitedBorders(currentIndex, (lowbound, upbound));
                _indexInteraction.ResizeIndex(currentIndex, a, b);

            }
            else
            {
                ProcessIndexLevel(currentIndex, page);
            }
            await _indexInteraction.FinalizeIndexChange();

        }

        public void AddValue(DataRecord<T> value)
        {
            throw new NotImplementedException();
        }

        public DataRecord<T> FindBefore(long stamp)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<DataRecord<T>[]> FindInRange(long start, long end)
        {
            
            if (start > end)
                return AsyncEnumerable.Empty<DataRecord<T>[]>();
            var pageQueue = new Queue<DataPageRef>();
            using (_indexSearch.ReadBlock())
            {
                var root = _indexSearch.GetRoot();
                if (!root.HasValue)
                    return AsyncEnumerable.Empty<DataRecord<T>[]>();
                if (root.Value.Start > end || root.Value.End<start)
                    return AsyncEnumerable.Empty<DataRecord<T>[]>();
                var indexQueue = new Queue<IndexRecord>();
                
                indexQueue.Enqueue(root.Value);
                do
                {
                    var curRec = indexQueue.Dequeue();
                    if (curRec.StoresData)
                        pageQueue.Enqueue(_indexSearch.GetDataRef(curRec));
                    else if (curRec.End >= start && curRec.Start <= end)
                    {
                        var t = _indexSearch.GetChildren(curRec);
                        indexQueue.Enqueue(t[0]);
                        indexQueue.Enqueue(t[1]);
                    }
                }
                while (indexQueue.Any());                
            }
            //в перечислении ниже индексы уже не используются
            return pageQueue.Select(k => _pageInteractor.FindRange(k, start, end)).ToAsyncEnumerable();
        }

        public void Remove(long stamp)
        {
            throw new NotImplementedException();
        }
    }
}
