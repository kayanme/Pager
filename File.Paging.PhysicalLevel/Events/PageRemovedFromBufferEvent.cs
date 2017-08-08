using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;

namespace File.Paging.PhysicalLevel.Events
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
