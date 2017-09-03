using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export(typeof(IPageFactory))]
    internal sealed class PageFactory : IPageFactory
    {
        private readonly HeaderPageFactory _headerPageFactory;
        private readonly LockingPageFactory _lockingPageFactory;
        private readonly PageContentFactory _pageContentFactory;
        private readonly LogicalLevelManipulationFactory _logicalLevelManipulationFactory;
        private readonly PageInfoFactory _pageInfoFactory;

        [ImportingConstructor]
        public PageFactory(HeaderPageFactory headerPageFactory, 
            LockingPageFactory lockingPageFactory, 
            PageContentFactory pageContentFactory, 
            LogicalLevelManipulationFactory logicalLevelManipulationFactory,
            PageInfoFactory pageInfoFactory)
        {
            _headerPageFactory = headerPageFactory;
            _lockingPageFactory = lockingPageFactory;
            _pageContentFactory = pageContentFactory;
            _logicalLevelManipulationFactory = logicalLevelManipulationFactory;
            _pageInfoFactory = pageInfoFactory;
        }

        public IPage<TRecord> GetRecordAccessor<TRecord>(BufferedPage page, PageReference pageNum,Action actionToClean) where TRecord : TypedRecord, new()
        {
            return _pageContentFactory.CreatePage<TRecord>(page, pageNum, actionToClean);
        }

        public IPage GetPageInfo(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return _pageInfoFactory.CreatePage(page, pageNum, actionToClean);
        }

        public IHeaderedPage<THeader> GetHeaderAccessor<THeader>(BufferedPage page, PageReference pageNum, Action actionToClean) where THeader : new()
        {
            return _headerPageFactory.CreatePage<THeader>(page, pageNum, actionToClean);
        }


        public IPhysicalLocks GetPageLocks(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return _lockingPageFactory.CreatePage(page, pageNum, actionToClean);
        }

        public ILogicalRecordOrderManipulation GetSorter(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return _logicalLevelManipulationFactory.CreatePage(page, pageNum, actionToClean);
        }



       
    }
}
