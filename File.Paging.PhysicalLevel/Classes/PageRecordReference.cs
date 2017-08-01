using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    public class PageReference
    {
        private int _pageNum;

        internal int PageNum => _pageNum;

        internal PageReference(int pageNum)
        {
            _pageNum = pageNum;
        }

        public override bool Equals(object obj)
        {
            return obj is PageReference && ((PageReference)obj)._pageNum == _pageNum;
        }

        public override int GetHashCode()
        {
            return _pageNum; 
        }

        public static bool operator ==(PageReference r1, PageReference r2) => r1?.PageNum == r2?.PageNum;

        public static bool operator !=(PageReference r1, PageReference r2) => r1?.PageNum != r2?.PageNum;

        public PageReference Copy()
        {
            return new PageReference(_pageNum);
        }
    }

    public class PageRecordReference
    {
        public PageReference Page { get; internal set; }
        internal int Record { get; set; }

        public static bool operator == (PageRecordReference r1,PageRecordReference r2)=>r1?.Page == r2?.Page && r1?.Record == r2?.Record;

        public static bool operator !=(PageRecordReference r1, PageRecordReference r2) => r1?.Page != r2?.Page || r1?.Record != r2?.Record;

        public override bool Equals(object obj)
        {
            var t = obj as PageRecordReference;
            if (t == null)
                return false;
            return t == this;
        }

        public override int GetHashCode()
        {
            return Page.GetHashCode() ^ Record;
        }

        public PageRecordReference Copy()
        {
            return new PageRecordReference { Record = Record, Page = Page.Copy() };
        }
    }
}
