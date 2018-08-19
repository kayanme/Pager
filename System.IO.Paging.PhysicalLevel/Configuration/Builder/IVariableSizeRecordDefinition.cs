namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    public interface IVariableSizeRecordDefinition<TRecordType>: IRecordDefinition<TRecordType> where TRecordType : struct
    {       
        int Size(TRecordType record);
    }
}