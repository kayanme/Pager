using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
   
    public interface IPageRecordTypeBuilder<TRecordType> where TRecordType : TypedRecord, new()
    {
        IFixedPageBuilder<TRecordType> UsingRecordDefinition(IFixedSizeRecordDefinition<TRecordType> recordDefinition);
        IFixedPageBuilder<TRecordType> UsingRecordDefinition(Action<TRecordType, byte[]> fillBytes,
            Action<byte[], TRecordType> fillFromBytes, int size);


        IVariablePageBuilder<TRecordType> WithMultipleTypeRecord(Func<TRecordType, byte> discriminatorFunction);
        IVariablePageWithOneRecordTypeBuilder UsingRecordDefinition(IVariableSizeRecordDefinition<TRecordType> recordDefinition);
        IVariablePageWithOneRecordTypeBuilder UsingRecordDefinition(Action<TRecordType, byte[]> fillBytes,
            Action<byte[], TRecordType> fillFromBytes, Func<TRecordType, int> size);
    }
}