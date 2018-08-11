using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeArchiver.Classes.Paging
{
    public struct DataPageRecord<T> where T:struct
    {
        public T Value;
        public ushort StampShift;
        public ushort VersionShift;
    }
}
