using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
{
    public interface IPageInfo : IDisposable
    {
        byte RegisteredPageType { get; }
        PageReference Reference { get; }
        double PageFullness { get; }
        int UsedRecords { get; }
        int ExtentNumber { get; }
    }
}