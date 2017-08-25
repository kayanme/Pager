using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using Rhino.Mocks.Constraints;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class LockingPage<TRecord>: LockingPageBase, 
        IPhysicalRecordManipulation,ILogicalRecordOrderManipulation,
        IPage<TRecord> where TRecord: TypedRecord,new()
    {
        private readonly IPage<TRecord> _pageImplementation;  
        private IPhysicalRecordManipulation _physicalLevelManipulationImplementation;
        private ILogicalRecordOrderManipulation _logicalLevelManipulationImplementation;
        private IPage<TRecord> _pageImplementation1;

        public void Dispose()
        {
            _pageImplementation.Dispose();
        }

        public LockingPage(IPage<TRecord> underlyingPage,
            IPhysicalLockManager<PageReference> pageLockManager,
            IPhysicalLockManager<PageRecordReference> pageRecordLockManager,
            LockMatrix lockMatrix):base(pageLockManager, pageRecordLockManager, lockMatrix)
        {
            _pageImplementation = underlyingPage;           
            _physicalLevelManipulationImplementation = _pageImplementation as IPhysicalRecordManipulation;
            _logicalLevelManipulationImplementation = _pageImplementation as ILogicalRecordOrderManipulation;
            ; Debug.Assert(_physicalLevelManipulationImplementation != null,"_physicalLevelManipulationImplementation != null");
        }

        public byte RegisteredPageType => _pageImplementation.RegisteredPageType;

        public override PageReference Reference => _pageImplementation.Reference;

        public double PageFullness => _pageImplementation.PageFullness;
        public int UsedRecords
        {
            get { return _pageImplementation1.UsedRecords; }
        }

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

        public void Flush()
        {
            _physicalLevelManipulationImplementation.Flush();
        }

      
        public void Compact()
        {
            _physicalLevelManipulationImplementation.Compact();
        }


        public void ApplyOrder(PageRecordReference[] records)
        {
            _logicalLevelManipulationImplementation.ApplyOrder(records);
        }

        public void DropOrder(PageRecordReference record)
        {
            _logicalLevelManipulationImplementation.DropOrder(record);
        }
    }
}
