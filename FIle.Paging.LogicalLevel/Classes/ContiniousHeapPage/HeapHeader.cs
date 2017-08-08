using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
    internal sealed class HeapHeader:TypedRecord,IFixedSizeRecordDefinition<HeapHeader>
    {
        public uint LogicalPageNum;
        public int Fullness;

        public void FillBytes(HeapHeader record, byte[] targetArray)
        {
            throw new NotImplementedException();
        }

        public void FillFromBytes(byte[] sourceArray, HeapHeader record)
        {
            throw new NotImplementedException();
        }

        public int Size => 4;
    }
}
