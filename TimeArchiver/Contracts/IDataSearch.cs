using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeArchiver.Contracts
{
    internal interface IDataSearch
    {
        Task CreateTag(long num, TagType type);
        Task InsertBlock(long num, DataRecord<int>[] block);
        IAsyncEnumerable<DataRecord<int>[]> FindInRangeInt(long tag, long start, long end);
    }
}
