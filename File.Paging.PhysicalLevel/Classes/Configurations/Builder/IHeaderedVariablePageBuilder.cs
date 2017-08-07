namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderedVariablePageBuilder<TRecordType> where TRecordType : TypedRecord, new()
    {
        IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType, IVariableSizeRecordDefinition<TRecordType> recordDefinition);
        IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType, IFixedSizeRecordDefinition<TRecordType> recordDefinition);
        IVariablePageBuilder<TRecordType> ApplyLogicalSortIndex();
        IVariablePageBuilder<TRecordType> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
    }
}