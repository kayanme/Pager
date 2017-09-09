using System;
using System.Linq;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
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