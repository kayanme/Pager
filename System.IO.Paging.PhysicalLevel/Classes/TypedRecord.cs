using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Classes
{
    public class TypedRecord<TRecord> where TRecord:struct
    {
        public PageRecordReference Reference { get; internal set; }
        internal int RecordStamp;
        public TRecord Data;
    }
}
