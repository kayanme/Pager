using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.IO.Paging.PhysicalLevel.Implementations.Headers;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
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
                    var used = pageCalculator.CalculateUsed(accessor.GetByteArray(0, pageCalculator.PamSize));
                    return new FixedRecordPhysicalOnlyHeader(headerAccessor, pageCalculator, used);
                }
                else
                {
                    var pageCalculator =
                        new FixedPageParametersCalculator((ushort) headerAccessor.PageSize, headerInfo.RecordSize, 16);
                    pageCalculator.CalculatePageParameters();                 
                    return new FixedRecordWithLogicalOrderHeader(headerAccessor, pageCalculator);
                }
            else
            {
                throw new Exception();
            }
        }
    }
}