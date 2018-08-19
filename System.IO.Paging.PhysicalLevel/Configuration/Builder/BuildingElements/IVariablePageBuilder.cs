namespace System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements
{
    public interface IVariablePageBuilder<TRecordType> where TRecordType : struct
    {
        IVariablePageBuilder<TRecordType> ApplyLogicalSortIndex();
        IVariablePageBuilder<TRecordType> ApplyLockScheme(LockRuleset locksRules);
        IHeaderedVariablePageBuilder<TRecordType, THeader> WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader : new();
    }
}