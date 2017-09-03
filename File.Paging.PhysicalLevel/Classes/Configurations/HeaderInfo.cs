namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal struct HeaderInfo
    {
        public HeaderInfo(bool isFixed, bool withLogicalSort, ushort recordSize)
        {
            IsFixed = isFixed;
            WithLogicalSort = withLogicalSort;
            RecordSize = recordSize;
        }

        public bool IsFixed { get; }
        public bool WithLogicalSort { get; }
        public ushort RecordSize { get; }
    }
}