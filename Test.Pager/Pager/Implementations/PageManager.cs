using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Classes;
using Pager.Implementations;

namespace Pager
{
    [Export(typeof(IPageManager))]
    internal sealed class PageManager : IPageManager
    {
       
        private IExtentAccessorFactory _blockFactory;
        private IGAMAccessor _accessor;
        private int _pageSize;
        private PageMapConfiguration _config;
        private IUnderlyingFileOperator _operatorForDisposal;
        private ConcurrentDictionary<int, BufferedPage> _bufferedPages = new ConcurrentDictionary<int, BufferedPage>();

        [ImportingConstructor]
        internal PageManager(PageMapConfiguration config,IGAMAccessor accessor,
            IExtentAccessorFactory blockFactory,IUnderlyingFileOperator operatorForDisposal)
        {
          
            _accessor = accessor;
              _blockFactory = blockFactory;
            _config = config;
            _pageSize = config.SizeOfPage == PageMapConfiguration.PageSize.Kb4 ? 4096 : 8192;
            _operatorForDisposal = operatorForDisposal;
        }      

        public FixedRecordTypedPage<TRecordType> CreatePage<TRecordType>() where TRecordType : TypedRecord,new()
        {
            var type = _config.PageMap.FirstOrDefault(k => k.Value == typeof(TRecordType)).Key;
            if (type == 0)
                throw new ArgumentException("TRecordType");
            var newPageNum = _accessor.MarkPageUsed(type);

            return RetrievePage<TRecordType>(new PageReference(newPageNum));
        }

        public void DeletePage(PageReference page, bool ensureEmptyness)
        {
            _accessor.MarkPageFree(page.PageNum);
        }

        public void Dispose()
        {
            foreach(var p in _bufferedPages)
            {
                p.Value.Accessor.Dispose();
                _accessor.Dispose();
                
            }
            _operatorForDisposal.Dispose();
        }

        public FixedRecordTypedPage<TRecordType> RetrievePage<TRecordType>(PageReference pageNum) where TRecordType : TypedRecord,new()
        {
            var page = _bufferedPages.GetOrAdd(pageNum.PageNum, i =>
            {
                var block = _blockFactory.GetAccessor(Extent.Size + i * _pageSize, _pageSize);
                
                var headers = new FixedRecordPageHeaders(block, new TRecordType().RecordSize);
                return new BufferedPage {Accessor = block,Headers =headers};
            });
            
            return new FixedRecordTypedPage<TRecordType>(page.Headers,page.Accessor, pageNum,_pageSize);
        }
    }
}
