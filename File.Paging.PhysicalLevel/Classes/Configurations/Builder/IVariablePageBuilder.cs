using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IVariablePageBuilder<TRecordType> where TRecordType : TypedRecord, new()
    {
        IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType,IVariableSizeRecordDefinition<TRecordType> recordDefinition);
        IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType, IFixedSizeRecordDefinition<TRecordType> recordDefinition);
        IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType, Action<TRecordType, byte[]> fillBytes,
            Action<byte[], TRecordType> fillFromBytes, Func<TRecordType, int> size);

        IVariablePageBuilder<TRecordType> ApplyLogicalSortIndex();
        IVariablePageBuilder<TRecordType> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);

        IHeaderedVariablePageBuilder<TRecordType> WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader : new();
    }
}