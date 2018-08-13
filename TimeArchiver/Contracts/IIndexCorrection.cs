using File.Paging.PhysicalLevel.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeArchiver.Contracts
{

    internal interface IIndexCorrection
    {
        IndexRecord? GetRoot();
        IndexRecord[] GetChildren(IndexRecord parent);
       
        void CreateDataBlock(DataPageRef records);       
        IndexRecord CreateDataBlock(IndexRecord root, DataPageRef records);        
        Task PrepareIndexChange();        
        
        IndexRecord MoveIndex(IndexRecord newRoot, IndexRecord recordToMove);        
        IndexRecord ResizeIndex(IndexRecord record, long start, long end);
        IndexRecord ResetTreeDepth(IndexRecord record);
        Task FinalizeIndexChange();
        PageRecordReference InitializeRoot();
    }    
}
