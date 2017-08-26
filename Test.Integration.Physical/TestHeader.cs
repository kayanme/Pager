using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace Test.Integration.Physical
{
    public class TestHeader
    {
        public string HeaderInfo;
    }

    public class TestHeaderGetter : IHeaderDefinition<TestHeader>
    {
        public unsafe void FillBytes(TestHeader record, byte[] targetArray)
        {
            
            fixed(char* src = record.HeaderInfo.ToCharArray())
            fixed (void* dst = targetArray)
            {
                Buffer.MemoryCopy(src, dst, Size, Size);                
            }

        }

        public unsafe void FillFromBytes(byte[] sourceArray, TestHeader record)
        {
            char* dst = stackalloc char[32];            
            fixed (void* src = sourceArray)
            {
                Buffer.MemoryCopy(src, dst, Size, Size);
                record.HeaderInfo = new string(dst);
            }

        }

        public int Size => sizeof(char)*32;
    }
}
