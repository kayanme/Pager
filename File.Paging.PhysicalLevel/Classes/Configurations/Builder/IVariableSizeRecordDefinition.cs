namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IVariableSizeRecordDefinition<TRecordType>: IRecordDefinition<TRecordType> where TRecordType : TypedRecord, new()
    {       
        int Size(TRecordType record);
    }
}