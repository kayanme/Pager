using System.IO.MemoryMappedFiles;

namespace System.IO.Paging.PhysicalLevel.Contracts.Internal
{
    internal interface IUnderlyingFileOperator:IDisposable
    {
        
    
        long FileSize { get; }
        MemoryMappedFile GetMappedFile(long desiredFileLength);
        void ReturnMappedFile(MemoryMappedFile file);
        void AddExtent(int extentCount);
    }
}
