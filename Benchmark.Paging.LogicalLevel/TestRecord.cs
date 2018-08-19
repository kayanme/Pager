using System;

namespace Benchmark.Paging.LogicalLevel
{
    public struct TestRecord
    {
        public int Order;

        public void FillFromByteArray(byte[] b)
        {
            Order = BitConverter.ToInt32(b, 0);
        }

        public void FillByteArray(byte[] b)
        {
            var t = BitConverter.GetBytes(Order);
            Array.Copy(t, b, 4);
        }
    }
}
