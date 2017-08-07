using System;
using System.IO.MemoryMappedFiles;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IUnderlyingFileOperator:IDisposable
    {
        
    
        long FileSize { get; }
        MemoryMappedFile GetMappedFile(long desiredFileLength);
        void ReturnMappedFile(MemoryMappedFile file);
        void AddExtent(int extentCount);
    }
}
