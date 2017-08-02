using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;

namespace Benchmark.Paging.LogicalLevel
{
    public class TestRecord:TypedRecord
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
