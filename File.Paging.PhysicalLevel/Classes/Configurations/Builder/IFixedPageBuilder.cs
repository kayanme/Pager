namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IFixedPageBuilder<TRecordType> where TRecordType :TypedRecord,new()
    {
        IFixedPageBuilder<TRecordType> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities);
       IHeaderedFixedPageBuilder<TRecordType,THeader> WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader:new();
    }
}