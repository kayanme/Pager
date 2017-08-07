using System;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IGamAccessor:IDisposable
    {
        void InitializeGam();
        byte GetPageType(int pageNum);
        int MarkPageUsed(byte pageType);
        void MarkPageFree(int pageNum);
         
    }
}
