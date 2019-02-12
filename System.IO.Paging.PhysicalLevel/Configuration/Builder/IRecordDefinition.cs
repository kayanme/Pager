namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    /// <summary>
    /// Base record operations definition.
    /// </summary>
    /// <typeparam name="TRecordType">Record type, which should be serialized</typeparam>
    public interface IRecordDefinition<TRecordType> where TRecordType :  new()
    {
        /// <summary>
        /// The way to fill target byte array from a source record (to store a record).
        /// </summary>
        /// <param name="record">Source record</param>
        /// <param name="targetArray">Target array</param>
        void FillBytes(ref TRecordType record, byte[] targetArray);
        /// <summary>
        /// The fill back the target record from a byte array (to read a record).
        /// </summary>
        /// <param name="sourceArray">Source array</param>
        /// <param name="record">Target record.</param>
        void FillFromBytes(byte[] sourceArray,ref TRecordType record);
    }
}