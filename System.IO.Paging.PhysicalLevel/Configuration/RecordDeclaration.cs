using System.Runtime.CompilerServices;

namespace System.IO.Paging.PhysicalLevel.Configuration
{
    internal abstract class RecordDeclaration
    {
        public abstract Type ClrType { get; }
        public bool IsVariableLength { get; protected set; }
    }
    public delegate void Setter<TRecordType>(byte[] source, ref TRecordType record);
    public delegate void Getter<TRecordType>(ref TRecordType record, byte[] target);
    internal abstract class RecordDeclaration<TRecordType> : RecordDeclaration
    {
        public sealed override Type ClrType => typeof(TRecordType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillFromBytes(byte[] bytes,ref TRecordType record) => _fillFromByte(bytes,ref record);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillBytes(ref TRecordType record, byte[] bytes) => _fillBytes(ref record, bytes);

       

        private readonly Getter<TRecordType> _fillBytes;
        private readonly Setter<TRecordType> _fillFromByte;
       
      

        protected RecordDeclaration(Getter<TRecordType> fillBytes, Setter<TRecordType> fillFromByte)
        {
            _fillBytes = fillBytes;
            _fillFromByte = fillFromByte;
           
         
        }
    }
}
