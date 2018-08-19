using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{

    [Export]
    internal sealed class HeaderPageFactory
    {
     

        public IHeaderedPage<THeaderType> CreatePage<THeaderType>(
           BufferedPage page,PageReference pageNum, Action actionToClean
        ) where THeaderType : new()
        {
            return new HeaderedPage<THeaderType>(page.Accessor, pageNum, page.HeaderConfig as PageHeadersConfiguration<THeaderType>,actionToClean);
        }
    }
}
