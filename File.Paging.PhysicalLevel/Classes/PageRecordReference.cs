namespace File.Paging.PhysicalLevel.Classes
{
    public class PageRecordReference
    {
        public PageReference Page { get; internal set; }
        internal int LogicalRecordNum { get; set; }

        internal PageRecordReference(int pageNum,int recordNum):this(new PageReference(pageNum),recordNum)
        {           
        }

        internal PageRecordReference(PageReference pageNum, int recordNum)
        {
            Page = pageNum;
            LogicalRecordNum = recordNum;
        }

        public static bool operator == (PageRecordReference r1,PageRecordReference r2)=>r1?.Page == r2?.Page && r1?.LogicalRecordNum == r2?.LogicalRecordNum;

        public static bool operator !=(PageRecordReference r1, PageRecordReference r2) => r1?.Page != r2?.Page || r1?.LogicalRecordNum != r2?.LogicalRecordNum;

        public override bool Equals(object obj)
        {
            var t = obj as PageRecordReference;
            if (t == null)
                return false;
            return t == this;
        }

        public override int GetHashCode()
        {
            return Page.GetHashCode() ^ LogicalRecordNum;
        }

        public override string ToString()
        {
            return $"Page {Page}, logical record {LogicalRecordNum} ";
        }

        public PageRecordReference Copy()
        {
            return new PageRecordReference(Page.Copy(), LogicalRecordNum);
        }
    }
}
