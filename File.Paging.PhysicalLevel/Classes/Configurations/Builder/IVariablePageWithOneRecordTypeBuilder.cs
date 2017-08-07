namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IVariablePageWithOneRecordTypeBuilder
    {
        IVariablePageWithOneRecordTypeBuilder ApplyLogicalSortIndex();
        IVariablePageWithOneRecordTypeBuilder WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
        IHeaderedVariablePageWithOneRecordBuilder WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader : new();
    }
}