using System.IO.Paging.PhysicalLevel.Configuration;

namespace System.IO.Paging.PhysicalLevel.Implementations.Serialization
{
    internal class SerializationCore<TRecord> where TRecord : new()
    {
        private readonly RecordDeclaration<TRecord> _definition;

        public SerializationCore(RecordDeclaration<TRecord> definition)
        {
            _definition = definition;
        }       


        public unsafe TRecord DeserializeFixedSize(byte* inputData,  int recordSize)
        {
           
            var t = new TRecord();
            var t2 = new byte[recordSize];

            fixed (byte* ft = t2)
            {
                Buffer.MemoryCopy(inputData, ft, recordSize, recordSize);
            }
            _definition.FillFromBytes(t2, ref t);

            return t;
        }

        public unsafe void SerializeFixedSize(byte* inputData,  int size, TRecord record)
        {
          
            var b = new byte[size];
            _definition.FillBytes(ref record, b);

            fixed (byte* ft = b)
            {
                Buffer.MemoryCopy(ft, inputData , size, size);
            }


        }
    }
}
