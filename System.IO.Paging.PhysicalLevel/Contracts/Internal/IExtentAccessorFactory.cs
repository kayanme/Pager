using System.IO.MemoryMappedFiles;

namespace System.IO.Paging.PhysicalLevel.Contracts.Internal
{
    internal interface IExtentAccessorFactory:IDisposable
    {
        IPageAccessor GetAccessor(long offset,int length);
        void ReturnAccessor(MemoryMappedViewAccessor map);
    }
}
