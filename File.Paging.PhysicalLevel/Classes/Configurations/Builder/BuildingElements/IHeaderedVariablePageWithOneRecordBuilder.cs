namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IHeaderedVariablePageWithOneRecordBuilder<TRecord, THeader>
    {
        IHeaderedVariablePageWithOneRecordBuilder<TRecord, THeader> ApplyLogicalSortIndex();
        IHeaderedVariablePageWithOneRecordBuilder<TRecord, THeader> ApplyLockScheme(LockRuleset locksRuleset);
    }
}