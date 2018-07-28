using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Timeserver.Pages
{
    internal sealed class DataRecord
    {
        public long Timestamp { get; set; }
        public double Value { get; set; }
    }
}
