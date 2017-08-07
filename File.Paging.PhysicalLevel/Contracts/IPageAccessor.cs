using System;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IPageAccessor : IDisposable
    {
     
        byte[] GetByteArray(int position, int length);
        void SetByteArray(byte[] record, int position, int length);
        int PageSize { get; }
        void Flush();
        uint ExtentNumber { get; }

        IPageAccessor GetChildAccessorWithStartShift(ushort startShirt);
    }
}
