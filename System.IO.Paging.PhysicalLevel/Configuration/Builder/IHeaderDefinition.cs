namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    public interface IHeaderDefinition<THeaderType> where THeaderType :  new()
    {
        void FillBytes(ref THeaderType record, byte[] targetArray);
        void FillFromBytes(byte[] sourceArray,ref THeaderType record);
        int Size { get; }
    }
}