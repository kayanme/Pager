namespace System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements
{
    public interface IHeaderedFixedPageBuilder<TRecord, THeader>
    {
        IHeaderedFixedPageBuilder<TRecord, THeader> ApplyLockScheme(LockRuleset locksRuleset);
    }
}