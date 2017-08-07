namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderedFixedPageBuilder<TRecord> where TRecord:TypedRecord,new()
    {
        IHeaderedFixedPageBuilder<TRecord> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
    }
}