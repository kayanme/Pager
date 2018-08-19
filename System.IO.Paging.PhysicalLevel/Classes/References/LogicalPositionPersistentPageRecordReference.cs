namespace System.IO.Paging.PhysicalLevel.Classes.References
{
    internal sealed class LogicalPositionPersistentPageRecordReference : PageRecordReference
    {
        internal LogicalPositionPersistentPageRecordReference(PageReference page, ushort logicalPosition) : base(page,
             logicalPosition)
        {

        }

        internal LogicalPositionPersistentPageRecordReference(int page, ushort logicalPosition) : base(page,
            logicalPosition)
        {

        }

        public override PageRecordReference Copy()
        {
            return new LogicalPositionPersistentPageRecordReference(Page,PersistentRecordNum);
        }
    }
}