using System;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IGamAccessor:IDisposable
    {
        void InitializeGam(ushort pageSize);
        byte GetPageType(int pageNum);
        int MarkPageUsed(byte pageType);
        void MarkPageFree(int pageNum);
        void SetPageType(int pageNum, byte pageType);

        long GamShift(int pageNum);
    }
}
