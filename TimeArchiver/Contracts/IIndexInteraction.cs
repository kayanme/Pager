using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeArchiver.Contracts
{



    internal interface IIndexInteraction<T> where T:struct
    {
        IndexRecord? GetRoot();
        IndexRecord[] GetChildren(IndexRecord parent);
        bool IsChildrenCapacityFull(IndexRecord record);
        void CreateDataBlock(DataRecord<T>[] records);
        IndexRecord CreateDataBlock(IndexRecord root, DataRecord<T>[] records);
        

        Task PrepareIndexChange();        
        IndexRecord CreateUnderlayingIndexRecord(IndexRecord record);
        IndexRecord MoveIndex(IndexRecord newRoot, IndexRecord recordToMove);
        void SwapIndexes(IndexRecord record1, IndexRecord record2);
        IndexRecord ResizeIndex(IndexRecord record, long start, long end);
        IndexRecord ResetTreeDepth(IndexRecord record);
        void FinalizeIndexChange();
    }    
}
