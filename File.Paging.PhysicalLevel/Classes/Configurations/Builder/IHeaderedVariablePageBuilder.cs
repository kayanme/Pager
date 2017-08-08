namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderedVariablePageBuilder<TRecord, THeader>
    {
       
        IHeaderedVariablePageBuilder<TRecord, THeader> ApplyLogicalSortIndex();
        IHeaderedVariablePageBuilder<TRecord, THeader> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
    }
}