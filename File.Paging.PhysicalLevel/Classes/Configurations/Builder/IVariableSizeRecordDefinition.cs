namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IVariableSizeRecordDefinition<TRecordType>: IRecordDefinition<TRecordType> where TRecordType : struct
    {       
        int Size(TRecordType record);
    }
}