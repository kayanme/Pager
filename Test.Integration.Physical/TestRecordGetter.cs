using System;
using System.Runtime.InteropServices;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace Test.Integration.Physical
{
    public class TestRecordGetter : IFixedSizeRecordDefinition<TestRecord>
    {
        public unsafe void FillBytes(TestRecord record, byte[] targetArray)
        {
            fixed (void* src = &record.Value)
            fixed (void* dst = targetArray)
            {
                Buffer.MemoryCopy(src, dst, Size, Size);
            }

        }

        public unsafe void FillFromBytes(byte[] sourceArray, TestRecord record)
        {

            fixed (void* src = sourceArray)
            fixed (void* dst = &record.Value)
            {
                Buffer.MemoryCopy(src, dst, Size, Size);
            }

        }

        public int Size => sizeof(long);
    }
}