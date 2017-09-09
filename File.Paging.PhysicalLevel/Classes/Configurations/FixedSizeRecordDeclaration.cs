using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal sealed class FixedSizeRecordDeclaration<TRecordType>: RecordDeclaration<TRecordType>
    {
        internal int GetSize { get; }

       
        public FixedSizeRecordDeclaration(Getter<TRecordType> fillBytes, Setter<TRecordType> fillFromByte, int size):base(fillBytes,fillFromByte)
        {

            GetSize =  size;
            IsVariableLength = false; 
        }
    }
}