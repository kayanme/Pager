namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderedVariablePageWithOneRecordBuilder
    {
        IVariablePageWithOneRecordTypeBuilder ApplyLogicalSortIndex();
        IVariablePageWithOneRecordTypeBuilder WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
    }
}