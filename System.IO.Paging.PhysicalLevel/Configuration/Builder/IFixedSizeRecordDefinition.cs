namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    public interface IFixedSizeRecordDefinition<TRecordType>: IRecordDefinition<TRecordType> where TRecordType : new()
    {
       
        int Size { get; }
    }
}