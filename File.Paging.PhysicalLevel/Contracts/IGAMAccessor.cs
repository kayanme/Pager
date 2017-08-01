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
        byte GetPageType(int pageNum);
        int MarkPageUsed(byte pageType);
        void MarkPageFree(int pageNum);
         
    }
}
