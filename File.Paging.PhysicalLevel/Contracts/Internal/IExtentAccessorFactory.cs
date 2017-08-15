using System;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IExtentAccessorFactory:IDisposable
    {
        Task<IPageAccessor> GetAccessor(long offset,int length);
        Task ReturnAccessor(MemoryMappedViewAccessor map);
    }
}
