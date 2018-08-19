namespace System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements
{
    public interface IFixedPageBuilder<TRecordType> where TRecordType :struct
    {
        IFixedPageBuilder<TRecordType> ApplyLogicalSortIndex();
       IFixedPageBuilder<TRecordType> ApplyLockScheme(LockRuleset locksRuleset);
       IHeaderedFixedPageBuilder<TRecordType,THeader> WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader:new();
    }
}