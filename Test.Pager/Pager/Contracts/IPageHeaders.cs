using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager.Contracts
{
    internal interface IPageHeaders
    {
        int TakeNewRecord();
        bool IsRecordFree(int record);
        void FreeRecord(int record);
        int RecordCount { get; }
        int HeaderSize { get; }
    }
}
