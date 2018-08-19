namespace System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements
{
    public interface IHeaderedVariablePageBuilder<TRecord, THeader>
    {
       
        IHeaderedVariablePageBuilder<TRecord, THeader> ApplyLogicalSortIndex();
        IHeaderedVariablePageBuilder<TRecord, THeader> ApplyLockScheme(LockRuleset locksRuleset);
    }
}