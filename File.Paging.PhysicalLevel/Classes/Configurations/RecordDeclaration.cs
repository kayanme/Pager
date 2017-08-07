using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal abstract class RecordDeclaration
    {
        public abstract Type ClrType { get; }
        public bool IsVariableLength { get; protected set; }
    }

    internal abstract class RecordDeclaration<TRecordType> : RecordDeclaration
    {
        public sealed override Type ClrType => typeof(TRecordType);

        public void FillFromBytes(byte[] bytes, TRecordType record) => _fillFromByte(bytes, record);

        public void FillBytes(TRecordType record, byte[] bytes) => _fillBytes(record, bytes);

       

        private readonly Action<TRecordType,byte[]> _fillBytes;
        private readonly Action<byte[], TRecordType> _fillFromByte;
       
      

        protected RecordDeclaration(Action<TRecordType, byte[]> fillBytes, Action<byte[], TRecordType> fillFromByte)
        {
            _fillBytes = fillBytes;
            _fillFromByte = fillFromByte;
           
         
        }
    }
}
