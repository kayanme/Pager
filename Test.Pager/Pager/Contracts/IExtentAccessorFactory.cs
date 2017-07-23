using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    internal interface IExtentAccessorFactory:IDisposable
    {
        IPageAccessor GetAccessor(long offset,int length);
        void ReturnAccessor(MemoryMappedViewAccessor map);
    }
}
