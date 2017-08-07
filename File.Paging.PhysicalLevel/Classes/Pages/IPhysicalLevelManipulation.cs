namespace File.Paging.PhysicalLevel.Classes.Pages
{
    public interface IPhysicalLevelManipulation
    {
        void Flush();
        void SwapRecords(PageRecordReference record1, PageRecordReference record2);
        void Compact();
    }
}
