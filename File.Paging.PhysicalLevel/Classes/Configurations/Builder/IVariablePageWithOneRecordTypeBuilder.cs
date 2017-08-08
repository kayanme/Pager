namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IVariablePageWithOneRecordTypeBuilder<TRecord>
    {
        IVariablePageWithOneRecordTypeBuilder<TRecord> ApplyLogicalSortIndex();
        IVariablePageWithOneRecordTypeBuilder<TRecord> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
        IHeaderedVariablePageWithOneRecordBuilder<TRecord, THeader> WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader : new();
    }
}