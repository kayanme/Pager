namespace System.IO.Paging.PhysicalLevel.Configuration.Builder
{
    /// <summary>
    /// Header definition.
    /// </summary>
    /// <typeparam name="THeaderType">Header type</typeparam>
    public interface IHeaderDefinition<THeaderType> where THeaderType :  new()
    {
        /// <summary>
        /// Serialize header into target byte array.
        /// </summary>
        /// <param name="record">Header</param>
        /// <param name="targetArray">Target array.</param>
        void FillBytes(ref THeaderType record, byte[] targetArray);
        /// <summary>
        /// Deserialize byte array into header
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <param name="record"></param>
        void FillFromBytes(byte[] sourceArray,ref THeaderType record);
        int Size { get; }
    }
}