using System;

namespace File.Paging.PhysicalLevel.Classes.Pages
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