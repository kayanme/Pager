﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    public abstract class TypedRecord
    {
        public PageRecordReference Reference { get; internal set; }
        internal int RecordStamp;
        public abstract short RecordSize { get; }
        public abstract void FillFromByteArray(IList<byte> b);
        public abstract void FillByteArray(IList<byte> b);
    }
}
