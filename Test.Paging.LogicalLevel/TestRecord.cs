using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;

namespace Test.Paging.LogicalLevel
{
    public class TestRecord:TypedRecord
    {
        public int Order;

        public override bool Equals(object obj)
        {
            var t = obj as TestRecord;
            if (t == null)
                return false;
            return Order == t.Order && Reference?.LogicalRecordNum == t.Reference?.LogicalRecordNum;
        }

        public override int GetHashCode()
        {
            return Order.GetHashCode() ^ Reference.LogicalRecordNum.GetHashCode();
        }
    }
}
