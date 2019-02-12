using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Implementations.Headers
{
    internal sealed class ImagePageHeader : IPageHeaders
    {
        
        private readonly IPageAccessor _accessor;        

        
        public ImagePageHeader(IPageAccessor accessor)
        {
            
            _accessor = accessor;            
        }


        
        public  IEnumerable<ushort> NonFreeRecords()
        {
            yield return 0;
        }

        
        
        public unsafe short TakeNewRecord(ushort rSize)
        {
            return -1;
        }    
     

        public unsafe bool IsRecordFree(ushort persistentRecordNum)
        {
            return false;
        }

        public unsafe void FreeRecord(ushort persistentRecordNum)
        {
           
        }

        public ushort RecordCount => 1;
        public ushort RecordShift(ushort persistentRecordNum) => 0;

        

        public ushort RecordSize(ushort persistentRecordNum) => (ushort)_accessor.PageSize;

        public void SetNewRecordInfo(ushort persistentRecordNum, ushort rSize)
        {           
        }

        public void ApplyOrder(ushort[] recordsInOrder)
        {
            throw new NotImplementedException();
        }

        public void DropOrder(ushort persistentRecordNum)
        {
            throw new NotImplementedException();
        }

      
        public void Compact()
        {
            throw new NotImplementedException();
        }

        public int TotalUsedSize => _accessor.PageSize;
    }
}
