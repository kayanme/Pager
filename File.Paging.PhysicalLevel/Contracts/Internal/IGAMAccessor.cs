using System;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IGamAccessor:IDisposable
    {
        void InitializeGam();
        Task<byte> GetPageType(int pageNum);
        Task<int> MarkPageUsed(byte pageType);
        Task MarkPageFree(int pageNum);
        Task SetPageType(int pageNum, byte pageType);
    }
}
