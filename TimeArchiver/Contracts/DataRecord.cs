namespace TimeArchiver.Contracts
{
    public struct DataRecord<T> where T:struct
    {
        public long Stamp;
        public T Data;
    }
}
