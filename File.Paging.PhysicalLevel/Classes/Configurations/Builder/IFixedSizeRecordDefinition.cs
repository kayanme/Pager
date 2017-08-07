namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IFixedSizeRecordDefinition<TRecordType>: IRecordDefinition<TRecordType> where TRecordType : TypedRecord, new()
    {
       
        int Size { get; }
    }
}