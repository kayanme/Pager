namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderDefinition<THeaderType> where THeaderType :  new()
    {
        void FillBytes(THeaderType record, byte[] targetArray);
        void FillFromBytes(byte[] sourceArray, THeaderType record);
        int Size { get; }
    }
}