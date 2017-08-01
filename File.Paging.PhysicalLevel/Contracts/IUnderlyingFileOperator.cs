using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    internal interface IUnderlyingFileOperator:IDisposable
    {
        
    
        long FileSize { get; }
        MemoryMappedFile GetMappedFile(long desiredFileLength);
        void ReturnMappedFile(MemoryMappedFile file);
        void AddExtent(int extentCount);
    }
}
