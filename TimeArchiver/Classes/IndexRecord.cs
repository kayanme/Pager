using File.Paging.PhysicalLevel.Classes;

namespace TimeArchiver.Contracts
{
    internal struct IndexRecord
    {
        public long Start;
        public long End;
        public bool StoresData;
        public short MaxUnderlyingDepth;

        internal PageRecordReference _recordNum;
        internal PageRecordReference _parentNum;
        internal byte TestKey;

        public IndexRecord(long start, long end, bool storesData, short maxUnderlyingDepth,byte testKey)
        {
            Start = start;
            End = end;
            StoresData = storesData;
            MaxUnderlyingDepth = maxUnderlyingDepth;
            _recordNum = null;
            TestKey = testKey;
            _parentNum = null;

       }


        public override string ToString()
        {
            return StoresData?$"{Start} - {End} (data)": $"{Start} - {End}";
        }
        public override bool Equals(object obj)
        {
            var t = (IndexRecord)obj;
            return Start == t.Start && End == t.End && StoresData == t.StoresData;
        }
    }
}
