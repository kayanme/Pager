using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
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
        private PageManagerConfiguration _config;
        private IUnderlyingFileOperator _operatorForDisposal;
        private ConcurrentDictionary<int, BufferedPage> _bufferedPages = new ConcurrentDictionary<int, BufferedPage>();
        private int _pages;
        [ImportingConstructor]
        internal PageManager(PageManagerConfiguration config,IGAMAccessor accessor,
            IExtentAccessorFactory blockFactory,IUnderlyingFileOperator operatorForDisposal)
        {
          
            _accessor = accessor;
            _blockFactory = blockFactory;
            _config = config;
            
            _pageSize = config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? 4096 : 8192;
            _operatorForDisposal = operatorForDisposal;
            _pages = (int)((operatorForDisposal.FileSize - Extent.Size) / _pageSize);            
        }

        private FixedRecordTypePageConfiguration<TRecordType> GetFixedConfig<TRecordType>() =>
            _config.PageMap.Values.OfType<FixedRecordTypePageConfiguration<TRecordType>>().FirstOrDefault(k => k.RecordType.ClrType == typeof(TRecordType));

        public FixedRecordTypedPage<TRecordType> CreatePage<TRecordType>() where TRecordType : TypedRecord,new()
        {
            var type = _config.PageMap.FirstOrDefault(k => (k.Value as FixedRecordTypePageConfiguration<TRecordType>)!=null).Key;
            if (type == 0)
                throw new ArgumentException("TRecordType");
            var newPageNum = _accessor.MarkPageUsed(type);
            Interlocked.Increment(ref _pages);
            return RetrievePage<TRecordType>(new PageReference(newPageNum));
        }

        public void DeletePage(PageReference page, bool ensureEmptyness)
        {
            _accessor.MarkPageFree(page.PageNum);
            Interlocked.Decrement(ref _pages);
        }

        public void Dispose()
        {
            foreach(var p in _bufferedPages)
            {
                p.Value.Accessor.Dispose();                           
            }
            _accessor.Dispose();
            _operatorForDisposal.Dispose();
        }

        public FixedRecordTypedPage<TRecordType> RetrievePage<TRecordType>(PageReference pageNum) where TRecordType : TypedRecord,new()
        {
            var page = _bufferedPages.GetOrAdd(pageNum.PageNum, i =>
            {
                var block = _blockFactory.GetAccessor(Extent.Size + i * _pageSize, _pageSize);
                var config = GetFixedConfig<TRecordType>();
                var headers = new FixedRecordPageHeaders(block,(ushort) config.RecordType.GetSize(null));
                return new BufferedPage {Accessor = block,Headers =headers,Config = config};
            });
            
            return new FixedRecordTypedPage<TRecordType>(page.Headers,page.Accessor, pageNum,_pageSize,page.Config as FixedRecordTypePageConfiguration<TRecordType>);
        }

        public void GroupFlush(params TypedPage[] pages)
        {
            foreach(var t in  pages.Select(k=>_bufferedPages[k.Reference.PageNum]).GroupBy(k=>k.Accessor.PageSize).Select(k=>k.First()))
            {
                t.Accessor.Flush();
            }
        }
    }
}
