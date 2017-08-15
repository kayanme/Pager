using System;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IPageAccessor : IDisposable
    {
     
        Task<byte[]> GetByteArray(int position, int length);
        Task SetByteArray(byte[] record, int position, int length);
        int PageSize { get; }
        Task Flush();
        uint ExtentNumber { get; }
        Task ClearPage();
        IPageAccessor GetChildAccessorWithStartShift(ushort startShirt);
    }
}
