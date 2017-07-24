using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    internal interface IPageAccessor:IDisposable
    {
        byte[] GetByteArray(int position,int length);
        void SetByteArray(byte[] record,int position, int length);
        int PageSize { get; }
        void Flush();
        uint ExtentNumber { get; }
    }
}
