using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager.Contracts
{
    internal interface IPageHeaders
    {
        short TakeNewRecord();
        bool IsRecordFree(ushort record);
        void FreeRecord(ushort record);
        ushort RecordCount { get; }
        byte HeaderSize { get; }
    }
}
