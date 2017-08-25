using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class LockingHeaderPage<TRecord> : LockingPageBase, IPhysicalRecordManipulation,IPage<TRecord>, IHeaderedPage<TRecord> where TRecord : TypedRecord, new()
    {
        private IPage<TRecord> _pageImplementation;
        private IHeaderedPage<TRecord> _headeredPageImplementation;
        private IPhysicalRecordManipulation _physicalLevelManipulationImplementation;
        private ILogicalRecordOrderManipulation _logicalLevelManipulationImplementation;

        public LockingHeaderPage(IPhysicalLockManager<PageReference> pageLockManager,
            IPhysicalLockManager<PageRecordReference> pageRecordLockManager, 
            LockMatrix lockMatrix, 
            IHeaderedPage<TRecord> pageImplementation) : base(pageLockManager, pageRecordLockManager, lockMatrix)
        {
            _pageImplementation = pageImplementation as IPage<TRecord>;
            _headeredPageImplementation = pageImplementation;
            _physicalLevelManipulationImplementation = pageImplementation as IPhysicalRecordManipulation;
            _logicalLevelManipulationImplementation = _pageImplementation as ILogicalRecordOrderManipulation;
        }

        public void Dispose()
        {
            _pageImplementation.Dispose();
        }

        public byte RegisteredPageType => _pageImplementation.RegisteredPageType;

        public double PageFullness => _pageImplementation.PageFullness;
        public int UsedRecords => _pageImplementation.UsedRecords;

        public bool AddRecord(TRecord type)
        {
            return _pageImplementation.AddRecord(type);
        }

        public void FreeRecord(TRecord record)
        {
            _pageImplementation.FreeRecord(record);
        }

        public TRecord GetRecord(PageRecordReference reference)
        {
            return _pageImplementation.GetRecord(reference);
        }

        public void StoreRecord(TRecord record)
        {
            _pageImplementation.StoreRecord(record);
        }

        public IEnumerable<PageRecordReference> IterateRecords()
        {
            return _pageImplementation.IterateRecords();
        }

        public TRecord GetHeader()
        {
            return _headeredPageImplementation.GetHeader();
        }

        public void ModifyHeader(TRecord header)
        {
            _headeredPageImplementation.ModifyHeader(header);
        }

        public void Flush()
        {
            _physicalLevelManipulationImplementation.Flush();
        }


        public void ApplyOrder(PageRecordReference[] records)
        {
            _logicalLevelManipulationImplementation.ApplyOrder(records);
        }

        public void Compact()
        {
            _physicalLevelManipulationImplementation.Compact();
        }

        public override PageReference Reference => _pageImplementation.Reference;
    }
}
