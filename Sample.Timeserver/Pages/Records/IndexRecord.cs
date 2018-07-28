using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;

namespace Sample.Timeserver.Pages.Records
{
    internal struct NotCompressedIndexRecord
    {
        public long StartStamp;
        public long EndStamp;
        public PageReference ChildPageReference;
    }
}
