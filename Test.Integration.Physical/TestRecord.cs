using File.Paging.PhysicalLevel.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Integration.Physical
{
    public class TestRecord:TypedRecord
    {
        public long Value;

        public TestRecord()
        {
            
        }

        public TestRecord(long value)
        {
            Value = value;
        }
    }
}
