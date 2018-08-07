using System.Threading.Tasks;

namespace TimeArchiver.Contracts
{
    internal interface IDataInteraction<T> where T:struct
    {
        Task AddBlock(DataRecord<T>[] sortedData);
        void AddValue(DataRecord<T> value);
        DataRecord<T>[] FindInRange(long start, long end);
        DataRecord<T> FindBefore(long stamp);
        void Remove(long stamp);
    }
}
