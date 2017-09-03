using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
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
