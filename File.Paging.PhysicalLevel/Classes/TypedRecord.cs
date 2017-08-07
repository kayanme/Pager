namespace File.Paging.PhysicalLevel.Classes
{
    public abstract class TypedRecord
    {
        public PageRecordReference Reference { get; internal set; }
        internal int RecordStamp;         
    }
}
