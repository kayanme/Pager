using System;
using System.Reflection;

namespace File.Paging.PhysicalLevel.Classes
{
    public abstract class PageRecordReference
    {
        public PageReference Page { get; internal set; }     

        internal ushort PersistentRecordNum { get; }
        
        internal PageRecordReference(int pageNum,ushort persistencRecordNum):this(new PageReference(pageNum), persistencRecordNum)
        {           
        }



        internal PageRecordReference(PageReference pageNum,ushort persistentRecordNum)
        {
            Page = pageNum;
            PersistentRecordNum = persistentRecordNum;    
        }

        public static bool operator == (PageRecordReference r1,PageRecordReference r2)=>
            r1?.Page == r2?.Page 
            && r1?.PersistentRecordNum == r2?.PersistentRecordNum;

        public static bool operator !=(PageRecordReference r1, PageRecordReference r2) => r1?.Page != r2?.Page || r1?.PersistentRecordNum != r2?.PersistentRecordNum;

        public override bool Equals(object obj)
        {
            var t = obj as PageRecordReference;
            if (t == null)
                return false;
            return t == this;
        }

        public override int GetHashCode()
        {
            return Page.GetHashCode()
                 ^ PersistentRecordNum.GetHashCode();
        }

        public override string ToString()
        {
            return $"Page {Page}, logical record {PersistentRecordNum} ";
        }

        public abstract PageRecordReference Copy();

    }
}
