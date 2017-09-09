using System;

namespace File.Paging.PhysicalLevel.Contracts
{
    internal interface IPageAccessor : IDisposable
    {
        byte[] GetWholePage();
        byte[] GetByteArray(int position, int length);        
        void SetByteArray(byte[] record, int position, int length);
        
        void QueueByteArrayOperation(int position, int length, ByteAction byteAction);

        int PageSize { get; }
        void Flush();
        uint ExtentNumber { get; }
        void ClearPage();
        IPageAccessor GetChildAccessorWithStartShift(ushort startShirt);
    }

    unsafe delegate void ByteAction(byte* ar);
}
