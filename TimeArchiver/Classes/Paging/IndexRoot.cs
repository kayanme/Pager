using File.Paging.PhysicalLevel.Classes;

namespace TimeArchiver.Contracts
{
    internal struct IndexRoot
    {
        public long TagNum;
        public PageRecordReference Root;
    }
}
