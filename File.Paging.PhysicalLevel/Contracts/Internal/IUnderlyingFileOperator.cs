using System;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IUnderlyingFileOperator:IDisposable
    {
        
    
        long FileSize { get; }
        Task<MemoryMappedFile> GetMappedFile(long desiredFileLength);
        Task ReturnMappedFile(MemoryMappedFile file);
        Task AddExtent(int extentCount);
    }
}
