namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class RowKeyPersistentPageRecordReference : PageRecordReference
    {
        internal RowKeyPersistentPageRecordReference(PageReference page, ushort key) : base(page, key)
        {

        }

        internal RowKeyPersistentPageRecordReference(int page, ushort key) : base(page, key)
        {

        }

        public override PageRecordReference Copy()
        {
            return new RowKeyPersistentPageRecordReference(Page, PersistentRecordNum);
        }
    }
}