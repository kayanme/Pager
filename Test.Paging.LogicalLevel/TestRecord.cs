

namespace Test.Paging.LogicalLevel
{
    public struct TestRecord
    {
        public int Order;

        public TestRecord(int order)
        {
            Order = order;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is TestRecord))
                return false;
            var t = (TestRecord)obj;
           
            return Order == t.Order;
        }

        public override int GetHashCode()
        {
            return Order.GetHashCode();
        }
    }
}
