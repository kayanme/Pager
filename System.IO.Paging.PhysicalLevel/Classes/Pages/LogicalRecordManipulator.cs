using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Linq;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed  class LogicalRecordManipulator : TypedPageBase,
        ILogicalRecordOrderManipulation
    {
        private readonly IPageHeaders _headers;

        public LogicalRecordManipulator(IPageHeaders headers,
            PageReference reference,
            Action actionToClean):base(reference, actionToClean)
        {
            _headers = headers;
        }

        public void ApplyOrder(PageRecordReference[] records)
        {
            _headers.ApplyOrder(records.Select(k => k.PersistentRecordNum).ToArray());
        }

        public void DropOrder(PageRecordReference record)
        {
            _headers.DropOrder(record.PersistentRecordNum);
        }

       
    }
}