using System.IO.Paging.PhysicalLevel.Configuration.Builder;

namespace System.IO.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
    internal struct HeapHeader:IFixedSizeRecordDefinition<HeapHeader>
    {
        public uint LogicalPageNum;
        public double Fullness;       

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case HeapHeader h when LogicalPageNum == h.LogicalPageNum && Fullness == h.Fullness: return true;
                default: return false;
            }
           
        }

        public override string ToString() => $"{LogicalPageNum} - {Fullness}";

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(LogicalPageNum * Fullness);
            }           
        }

        public void FillBytes(ref HeapHeader record, byte[] targetArray)
        {
            var b = BitConverter.GetBytes(record.LogicalPageNum);
            b[3] &= 0xC;
         
            switch (record.Fullness)
            {
                case double t when t >= 0 && t < .50:
                    b[3] |= 0;
                    break;
                case double t when t >= .50 && t < .80:
                    b[3] |= 1;
                    break;
                case double t when t >= .80 && t < .95:
                    b[3] |= 2;
                    break;
                case double t when t >= .95:
                    b[3] |= 3;
                    break;
            }
            Array.Copy(b,targetArray,4);
        }

        public void FillFromBytes(byte[] sourceArray,ref HeapHeader record)
        {
            var fb = sourceArray[3];
            sourceArray[3] &= 0xC;
            record.LogicalPageNum = BitConverter.ToUInt32(sourceArray,0);
            switch (fb & 0x3)
            {
                case 0:
                    record.Fullness = 0;
                    break;
                case 1:
                    record.Fullness = .50;
                    break;
                case 2:
                    record.Fullness = .80;
                    break;
                case 3:
                    record.Fullness = .95;
                    break;
            }
        }

        public int Size => 4;
    }
}
