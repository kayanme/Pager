using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TimeArchiver.Classes.Paging
{
    public struct DataPageRecord<T> where T:struct
    {
        public T Value;
        public ushort StampShift;
        public ushort VersionShift;

        public static readonly int MaxRecordsOnPage = 4096/Marshal.SizeOf<T>() + sizeof(ushort) * 2;
    }
}
