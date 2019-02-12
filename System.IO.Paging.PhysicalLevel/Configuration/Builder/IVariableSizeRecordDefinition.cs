namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    /// <summary>
    /// Provider for the record with a variable size.
    /// </summary>
    /// <typeparam name="TRecordType">Record type</typeparam>
    public interface IVariableSizeRecordDefinition<TRecordType>: IRecordDefinition<TRecordType> where TRecordType : struct
    {       
        /// <summary>
        /// Get size for the seriazing record
        /// </summary>
        /// <param name="record">Record</param>
        /// <returns>Size</returns>
        int Size(TRecordType record);
    }
}