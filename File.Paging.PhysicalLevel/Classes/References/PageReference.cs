namespace File.Paging.PhysicalLevel.Classes
{
    public class PageReference
    {
        internal int PageNum { get; }

        

        internal PageReference(int pageNum)
        {
            PageNum = pageNum;
        }

        public override bool Equals(object obj)
        {
            return obj is PageReference && ((PageReference)obj).PageNum == PageNum;
        }

        public override string ToString()
        {
            return "logical num: " + PageNum;
        }

        public override int GetHashCode()
        {
            return PageNum; 
        }

        public static bool operator ==(PageReference r1, PageReference r2) => r1?.PageNum == r2?.PageNum;

        public static bool operator !=(PageReference r1, PageReference r2) => r1?.PageNum != r2?.PageNum;

        public PageReference Copy()
        {
            return new PageReference(PageNum);
        }
    }
}