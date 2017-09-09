namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderDefinition<THeaderType> where THeaderType :  new()
    {
        void FillBytes(ref THeaderType record, byte[] targetArray);
        void FillFromBytes(byte[] sourceArray,ref THeaderType record);
        int Size { get; }
    }
}