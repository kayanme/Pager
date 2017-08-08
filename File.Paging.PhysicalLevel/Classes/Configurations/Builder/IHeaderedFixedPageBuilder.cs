namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderedFixedPageBuilder<TRecord, THeader> 
    {
        IHeaderedFixedPageBuilder<TRecord, THeader> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
    }
}