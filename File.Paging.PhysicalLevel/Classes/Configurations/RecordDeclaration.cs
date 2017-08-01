using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager.Classes
{
    public abstract class RecordDeclaration
    {
        public abstract Type ClrType { get; }
        public bool IsVariableLength { get; protected set; }
    }

    public abstract class RecordDeclaration<TRecordType> : RecordDeclaration
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

    public class FixedSizeRecordDeclaration<TRecordType>: RecordDeclaration<TRecordType>
    {
        public int GetSize { get; }

        public FixedSizeRecordDeclaration(Action<TRecordType, byte[]>fillBytes, Action<byte[], TRecordType> fillFromByte, int size):base(fillBytes,fillFromByte)
        {

            GetSize =  size;
            IsVariableLength = false;
        }
    }

    public class VariableSizeRecordDeclaration<TRecordType> : RecordDeclaration<TRecordType>
    {
        public int GetSize(TRecordType record) => _sizeGet(record);
        private readonly Func<TRecordType, int> _sizeGet;

    
        public VariableSizeRecordDeclaration(Action<TRecordType, byte[]> fillBytes, Action<byte[], TRecordType> fillFromByte, 
            Func<TRecordType, int> sizeGet):base(fillBytes,fillFromByte)
        {
           
            _sizeGet = sizeGet;
            IsVariableLength = true;
        }
    }

  
}
