namespace System.IO.Paging.PhysicalLevel.Contracts.Internal
{
    internal interface IRecordAcquirer<TRecord> where TRecord : struct
    {
        TRecord GetRecord(ushort offset, ushort size);
        void SetRecord(ushort offset, ushort size, TRecord record);
        void Flush();
    }
}
