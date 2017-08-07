namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IRecordDefinition<TRecordType> where TRecordType : TypedRecord, new()
    {
        void FillBytes(TRecordType record, byte[] targetArray);
        void FillFromBytes(byte[] sourceArray, TRecordType record);
    }
}