using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Exceptions;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{

    [Export]
    internal sealed class HeaderPageFactory
    {
     

        public IHeaderedPage<THeaderType> CreatePage<THeaderType>(
           BufferedPage page,PageReference pageNum, Action actionToClean
        ) where THeaderType : new()
        {
            if (page == null)
                return null;
            if (page.HeaderConfig == null)
                throw new NoAccessorAvailableException($"No headers configured for the page type {page.PageType}");
            if (!(page.HeaderConfig is PageHeadersConfiguration<THeaderType>))
                throw new RecordTypeDoesNotMatchesConfigurationException($"Requested header accessor for type {typeof(THeaderType)} does not match configuration header type {page.HeaderConfig.GetType().GetGenericArguments()[0]} ");
            return new HeaderedPage<THeaderType>(page.Accessor, pageNum, page.HeaderConfig as PageHeadersConfiguration<THeaderType>,actionToClean);
        }
    }
}
