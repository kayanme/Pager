using System;
using System.IO.MemoryMappedFiles;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IExtentAccessorFactory:IDisposable
    {
        IPageAccessor GetAccessor(long offset,int length);
        void ReturnAccessor(MemoryMappedViewAccessor map);
    }
}
