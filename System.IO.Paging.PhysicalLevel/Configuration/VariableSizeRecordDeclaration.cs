namespace System.IO.Paging.PhysicalLevel.Configuration
{
    internal sealed class VariableSizeRecordDeclaration<TRecordType> : RecordDeclaration<TRecordType>
    {
        public int GetSize(TRecordType record) => _sizeGet(record);
        private readonly Func<TRecordType, int> _sizeGet;

    
        public VariableSizeRecordDeclaration(Getter<TRecordType> fillBytes, Setter<TRecordType> fillFromByte, 
            Func<TRecordType, int> sizeGet):base(fillBytes,fillFromByte)
        {
           
            _sizeGet = sizeGet;
            IsVariableLength = true;
        }
    }
}