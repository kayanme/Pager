namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal interface IHeaderedPageInt<TRecord> : IPage where TRecord:TypedRecord,new()
    {
        void SwapContent(IPage<TRecord> page);
    }
}