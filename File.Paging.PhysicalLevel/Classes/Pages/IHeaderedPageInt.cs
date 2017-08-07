namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal interface IHeaderedPageInt : IPage
    {
        void SwapContent(IPage page);
    }
}