using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal abstract class TypedPageBase<TRecordType> : TypedPageBase, IPage<TRecordType> where TRecordType : TypedRecord, new()

    {
        private readonly int _pageSize;
        public override double PageFullness => (double)Headers.TotalUsedSize / _pageSize;

        protected TypedPageBase(IPageHeaders headers, IPageAccessor accessor, PageReference reference, byte pageType,int pageSize) : base(headers, accessor, reference, pageType)
        {
            _pageSize = pageSize;
        }

        public int UsedRecords => Headers.RecordCount;

        public abstract bool AddRecord(TRecordType type);

        public void FreeRecord(TRecordType record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (record.Reference is NullPageRecordReference)
                throw new ArgumentException("Trying to delete deleted record");
            try
            {
                CompactionLock.EnterReadLock();
                Headers.FreeRecord((ushort)record.Reference.PersistentRecordNum);
                record.Reference = new NullPageRecordReference(Reference);
            }
            finally
            {
                CompactionLock.ExitReadLock();
            }
        }

        public abstract TRecordType GetRecord(PageRecordReference reference);

        public abstract void StoreRecord(TRecordType record);

        public abstract IEnumerable<PageRecordReference> IterateRecords();
    }
}
