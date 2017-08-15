namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderedFixedPageBuilder<TRecord, THeader>
    {
        IHeaderedFixedPageBuilder<TRecord, THeader> ApplyLockScheme(LockRuleset locksRuleset);
    }
}