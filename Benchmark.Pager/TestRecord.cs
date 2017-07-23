using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager;

namespace Benchmark.Pager
{
    public class TestRecord:TypedRecord
    {
        public byte[] Values = new byte[7];

        public override short RecordSize
        {
            get
            {
                return 7;
            }
        }

        public override void FillByteArray(IList<byte> b)
        {
          for (int i=0;i<RecordSize;i++)
            {
                b[i] = Values[i];
            }
        }

        public override void FillFromByteArray(IList<byte> b)
        {

            for (int i = 0; i < RecordSize; i++)
            {
                Values[i] = b[i];
            }
        }
    }
}
