using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export]
    internal sealed class PageInfoFactory
    {
        public IPage CreatePage(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return new PageInfo(pageNum,page.Headers,page.PageType,(ushort)page.ContentAccessor.PageSize,page.Accessor.ExtentNumber, actionToClean);
        }
    }
}
