using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export(typeof(IPageFactory))]
    internal sealed class PageFactory : IPageFactory
    {
        private readonly HeaderPageFactory _headerPageFactory;
        private readonly LockingPageFactory _lockingPageFactory;
        private readonly PageContentFactory _pageContentFactory;
        private readonly LogicalLevelManipulationFactory _logicalLevelManipulationFactory;
        private readonly PageInfoFactory _pageInfoFactory;
        private readonly BinarySearcherFactory _binarySearcherFactory;

        [ImportingConstructor]
        public PageFactory(HeaderPageFactory headerPageFactory, 
            LockingPageFactory lockingPageFactory, 
            PageContentFactory pageContentFactory, 
            LogicalLevelManipulationFactory logicalLevelManipulationFactory,
            PageInfoFactory pageInfoFactory, BinarySearcherFactory binarySearcherFactory)
        {
            _headerPageFactory = headerPageFactory;
            _lockingPageFactory = lockingPageFactory;
            _pageContentFactory = pageContentFactory;
            _logicalLevelManipulationFactory = logicalLevelManipulationFactory;
            _pageInfoFactory = pageInfoFactory;
            _binarySearcherFactory = binarySearcherFactory;
        }

        public IPage<TRecord> GetRecordAccessor<TRecord>(BufferedPage page, PageReference pageNum,Action actionToClean) where TRecord : struct
        {
            return _pageContentFactory.CreatePage<TRecord>(page, pageNum, actionToClean);
        }

        public IBinarySearcher<TRecord> GetBinarySearcher<TRecord>(BufferedPage page, PageReference pageNum, Action actionToClean) where TRecord : struct
        {
            return _binarySearcherFactory.CreatePage<TRecord>(page, pageNum, actionToClean);
        }

        public IPageInfo GetPageInfo(BufferedPage page, PageReference pageNum, Action actionToClean)
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

        public ILogicalRecordOrderManipulation GetSorter<TRecord>(BufferedPage page, PageReference pageNum, Action actionToClean) where TRecord:struct
        {
            return _logicalLevelManipulationFactory.CreatePage<TRecord>(page, pageNum, actionToClean);
        }



       
    }
}
