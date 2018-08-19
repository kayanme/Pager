using System;
using System.Collections.Generic;

namespace Benchmark.Paging.PhysicalLevel
{
    public unsafe struct TestRecord
    {
        public fixed byte Values[7];

        public ushort RecordSize => 7;

        public uint IntValue
        {
            get
            {
                uint t = 0;
                unchecked
                {
                    fixed (byte* v = Values)
                        for (int i = 3; i >= 0; i--)
                        {
                            t |= (uint)v[i] << (i * 8);
                        }
                }
                return t;
            }

            set
            {
                uint t = 0;
                unchecked
                {
                    var bytes = BitConverter.GetBytes(value);
                    fixed (byte* v = Values)
                        for (int i = 3; i >= 0; i--)
                        {
                            v[i] = bytes[i];
                        }
                }
                
            }

        }

        public TestRecord(byte[] data)
        {
            fixed (byte* t = Values)
                for (int i = 0; i < RecordSize; i++)
                {
                    *(t + i) = data[i];
                }
        }

        public void Change(int i,byte v)
        {
            fixed (byte* t = Values)
               
                    *(t + i) = v;
                
        }

        public void FillByteArray(IList<byte> b)
        {
            fixed(byte* t = Values)
            for (int i = 0; i < RecordSize; i++)
            {
                b[i] = t[i];
            }
        }

        public void FillFromByteArray(IList<byte> b)
        {
            fixed (byte* t = Values)
                for (int i = 0; i < RecordSize; i++)
            {
                *(t + i) = b[i];
            }
        }
    }
}
