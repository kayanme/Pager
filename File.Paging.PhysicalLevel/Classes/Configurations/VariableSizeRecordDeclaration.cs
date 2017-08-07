using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal sealed class VariableSizeRecordDeclaration<TRecordType> : RecordDeclaration<TRecordType>
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