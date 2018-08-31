using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.PageFactories;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Text;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IBufferedPageFactory))]
    internal sealed class BufferedPageFactory : IBufferedPageFactory
    {
        private readonly IExtentAccessorFactory _blockFactory;
        private readonly IGamAccessor _accessor;
        private readonly IHeaderFactory _headerFactory;

        [ImportingConstructor]
        public BufferedPageFactory(IHeaderFactory headerFactory, IExtentAccessorFactory blockFactory,IGamAccessor accessor)
        {
            _blockFactory = blockFactory;
            _accessor = accessor;
            _headerFactory = headerFactory;
          
        }

        public BufferedPage CreateHeaderedPage(int pageNum, PageContentConfiguration pageConfig, PageHeadersConfiguration headerConfig, int _pageSize)
        {
            var block = _blockFactory.GetAccessor(_accessor.GamShift(pageNum) + (long)pageNum * _pageSize, _pageSize);
            var type = headerConfig.InnerPageMap;
            if (type == null)
                throw new InvalidOperationException();
            var headers = _headerFactory.CreateHeaders(type, block, headerConfig);

            return new BufferedPage
            {
                Accessor = block,
                ContentAccessor = block.GetChildAccessorWithStartShift(headerConfig.HeaderSize),
                Headers = headers,
                Config = type,
                HeaderConfig = headerConfig
            };
        }

        public BufferedPage CreatePage(int pageNum, PageContentConfiguration pageConfig, int _pageSize)
        {
            var block = _blockFactory.GetAccessor(_accessor.GamShift(pageNum) + (long)pageNum * _pageSize, _pageSize);                              
            var headers = _headerFactory.CreateHeaders(pageConfig, block);
            return
            new BufferedPage
            {
                Accessor = block,
                ContentAccessor = block,
                Headers = headers,
                Config = pageConfig
            };

        }
    }
}
