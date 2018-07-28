using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Timeserver.Pages.Records
{
    internal struct NotCompressedDataRecord
    {
        public long Stamp;
        public double Value;
    }

    internal struct CompressedBase4DataRecord
    {
        public ushort Stamp;
        public double Value;
    }
}
