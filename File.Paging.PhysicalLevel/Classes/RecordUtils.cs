using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes
{
    public static class RecordUtils
    {
        public  static unsafe string StringFromBytes(byte[] data, int position, int stringLength)
        {
            char* dst = stackalloc char[stringLength];
            fixed (byte* src = data)
            {
                Buffer.MemoryCopy(src+position, dst, stringLength*2, stringLength * 2);
                return new string(dst, 0, stringLength);
            }
        }

        public static unsafe void StringToBytes(byte[] data, int position, string source, int stringLength)
        {
            fixed (char* src = source.ToCharArray())
            fixed (byte* dst = data)
            {
                Buffer.MemoryCopy(src, dst+ position, stringLength * 2, stringLength * 2);
            }
        }

        public static unsafe void FromBytes(byte[] data, int position, ref long value)
        {
            
            fixed (byte* src = data)
            {
                long* dst = (long*)value;
                Buffer.MemoryCopy(src + position, dst, sizeof(long), sizeof(long));              
            }
        }

        public static unsafe void ToBytes(ref long source,byte[] data, int position)
        {                      
            fixed (byte* dst = data)
            {
                long* src = (long*) source;
                Buffer.MemoryCopy(src, dst+ position, sizeof(long), sizeof(long));
            }
        }

      
    }
}
