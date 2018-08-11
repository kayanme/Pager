namespace TimeArchiver.Contracts
{
    public struct DataRecord<T> where T:struct
    {
        public long VersionStamp;
        public long Stamp;
        public T Data;
    }
}
