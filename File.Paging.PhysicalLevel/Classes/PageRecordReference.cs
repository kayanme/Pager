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

        public override string ToString()
        {
            return "logical num: " + _pageNum;
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
        internal int LogicalRecordNum { get; set; }
        

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
            return new PageRecordReference { LogicalRecordNum = LogicalRecordNum, Page = Page.Copy() };
        }
    }
}
