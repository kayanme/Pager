using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    internal interface IGAMAccessor:IDisposable
    {
        void InitializeGAM();

        int MarkPageUsed(byte pageType);
        void MarkPageFree(int pageNum);
         
    }
}
