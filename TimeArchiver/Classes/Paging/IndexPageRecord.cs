using File.Paging.PhysicalLevel.Classes;

namespace TimeArchiver.Contracts
{
    internal struct IndexPageRecord
    {
        public long Start;
        public long End;        
        public short MaxUnderlyingDepth;

        public PageRecordReference ChildrenOne;
        public PageRecordReference ChildrenTwo;

        public PageReference Data;
    }
}
