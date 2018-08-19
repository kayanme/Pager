namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    public interface IRecordDefinition<TRecordType> where TRecordType :  new()
    {
        void FillBytes(ref TRecordType record, byte[] targetArray);
        void FillFromBytes(byte[] sourceArray,ref TRecordType record);
    }
}