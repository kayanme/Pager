using System.IO.Paging.PhysicalLevel.Classes.References;
using System.Runtime.CompilerServices;

namespace System.IO.Paging.PhysicalLevel.Classes
{
    public static class RecordUtils
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FromBytes(byte[] data, int position,ref PageRecordReference dest)
        {
            ushort num = 0;
            int page = 0;            
            FromBytes(data, position, ref num);
            FromBytes(data, position+sizeof(ushort), ref page);
            var pageRef =  new PageReference(page);
            dest =  PageRecordReference.CreateReference(new PageReference(page), num,Pages.KeyPersistanseType.Physical);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToBytes(ref PageRecordReference source,byte[] data, int position)
        {
            var num = source.PersistentRecordNum;
            int page;
          
            page = source.Page.PageNum;
            ToBytes(ref num, data, position);
            ToBytes(ref page, data, position+sizeof(ushort));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FromBytes(byte[] data, int position, ref PageReference dest)
        {
            
            int page = 0;
            
            FromBytes(data, position, ref page);
            var pageRef = new PageReference(page);
            dest =new PageReference(page);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToBytes(ref PageReference source, byte[] data, int position)
        {
         
            int page;
            page = source.PageNum;            
            ToBytes(ref page, data, position);
        }

        public const byte RecordReferenceLength = sizeof(ushort) + sizeof(int);
        public const byte PageReferenceLength =  sizeof(int);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  static unsafe string StringFromBytes(byte[] data, int position, int stringLength)
        {
            char* dst = stackalloc char[stringLength];
            fixed (byte* src = data)
            {
                Buffer.MemoryCopy(src+position, dst, stringLength*2, stringLength * 2);
                return new string(dst, 0, stringLength);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void StringToBytes(byte[] data, int position, string source, int stringLength)
        {
            fixed (char* src = source.ToCharArray())
            fixed (byte* dst = data)
            {
                Buffer.MemoryCopy(src, dst+ position, stringLength * 2, stringLength * 2);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FromBytes(byte[] data, int position, ref long value)
        {
            
            fixed (byte* src = data)
            fixed (void* dst = &value)
            {               
                Buffer.MemoryCopy(src + position, dst, sizeof(long), sizeof(long));              
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToBytes(ref long source,byte[] data, int position)
        {                      
           
            fixed (byte* dst = data)
            fixed (void* src = &source)
            {

                Buffer.MemoryCopy(src, dst + position, sizeof(long), sizeof(long));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FromBytes(byte[] data, int position, ref short value)
        {

            fixed (byte* src = data)
            fixed (void* dst = &value)
            {
                Buffer.MemoryCopy(src + position, dst, sizeof(short), sizeof(short));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToBytes(ref short source, byte[] data, int position) 
        {

            fixed (byte* dst = data)
            fixed (void* src = &source)
            {

                Buffer.MemoryCopy(src, dst + position, sizeof(short), sizeof(short));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FromBytes(byte[] data, int position, ref ushort value)
        {

            fixed (byte* src = data)
            fixed (void* dst = &value)
            {
                Buffer.MemoryCopy(src + position, dst, sizeof(ushort), sizeof(ushort));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToBytes(ref ushort source, byte[] data, int position)
        {

            fixed (byte* dst = data)
            fixed (void* src = &source)
            {

                Buffer.MemoryCopy(src, dst + position, sizeof(ushort), sizeof(ushort));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FromBytes(byte[] data, int position, ref int value)
        {

            fixed (byte* src = data)
            fixed (void* dst = &value)
            {
                Buffer.MemoryCopy(src + position, dst, sizeof(int), sizeof(int));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToBytes(ref int source, byte[] data, int position)
        {

            fixed (byte* dst = data)
            fixed (void* src = &source)
            {

                Buffer.MemoryCopy(src, dst + position, sizeof(int), sizeof(int));
            }
        }
    }
}
