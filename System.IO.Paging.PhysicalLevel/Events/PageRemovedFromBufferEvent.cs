using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Events
{
    public  class PageRemovedFromBufferEventArgs:EventArgs
    {
        public PageReference Page { get; }

        public PageRemovedFromBufferEventArgs(PageReference page)
        {
            Page = page;
        }
    }

    public delegate void PageRemovedFromBufferEventHandler(object sender, PageRemovedFromBufferEventArgs eventArgs);
}
