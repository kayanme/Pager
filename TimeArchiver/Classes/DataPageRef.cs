using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;

namespace TimeArchiver.Contracts
{
    internal struct DataPageRef
    {
        public long Start;
        public long End;
        public PageReference DataReference;
    }
}
