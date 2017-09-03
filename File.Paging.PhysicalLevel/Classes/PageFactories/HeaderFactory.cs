using System;
using System.ComponentModel.Composition;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations.Headers;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export(typeof(IHeaderFactory))]
    internal sealed class HeaderFactory : IHeaderFactory
    {
        public IPageHeaders CreateHeaders(PageContentConfiguration config,
            IPageAccessor accessor, PageHeadersConfiguration headerConfig = null)
        {
            var startPageShift = headerConfig?.HeaderSize ?? 0;
            var headerInfo = config.ReturnHeaderInfo();
            var headerAccessor = accessor.GetChildAccessorWithStartShift(startPageShift);
            if (headerInfo.IsFixed)
                if (!headerInfo.WithLogicalSort)
                {
                    var pageCalculator =
                        new FixedPageParametersCalculator((ushort) headerAccessor.PageSize, headerInfo.RecordSize);
                    pageCalculator.CalculatePageParameters();
                    var rawPam = headerAccessor.GetByteArray(0, pageCalculator.PamSize);
                    pageCalculator.ProcessPam(rawPam);
                    return new FixedRecordPhysicalOnlyHeader(headerAccessor, pageCalculator);
                }
                else
                {
                    var pageCalculator =
                        new FixedPageParametersCalculator((ushort) headerAccessor.PageSize, headerInfo.RecordSize, 16);
                    pageCalculator.CalculatePageParameters();
                    var rawPam = headerAccessor.GetByteArray(0, pageCalculator.PamSize);
                    pageCalculator.ProcessPam(rawPam);
                    return new FixedRecordWithLogicalOrderHeader(headerAccessor, pageCalculator);
                }
            else
            {
                throw new Exception();
            }
        }
    }
}