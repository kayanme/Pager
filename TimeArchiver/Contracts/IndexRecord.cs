namespace TimeArchiver.Contracts
{
    internal struct IndexRecord
    {
        public long Start;
        public long End;
        public bool StoresData;
        public int MaxUnderlyingDepth;

        public IndexRecord(long start, long end, bool storesData, int maxUnderlyingDepth)
        {
            Start = start;
            End = end;
            StoresData = storesData;
            MaxUnderlyingDepth = maxUnderlyingDepth;
        }

        public override bool Equals(object obj)
        {
            var t = (IndexRecord)obj;
            return Start == t.Start && End == t.End && StoresData == t.StoresData;
        }
    }
}
