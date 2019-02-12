namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    /// <summary>
    /// Fixed size record definition.
    /// </summary>
    /// <typeparam name="TRecordType"></typeparam>
    public interface IFixedSizeRecordDefinition<TRecordType>: IRecordDefinition<TRecordType> where TRecordType : new()
    {
       /// <summary>
       /// The size of record in bytes.
       /// </summary>
        int Size { get; }
    }
}