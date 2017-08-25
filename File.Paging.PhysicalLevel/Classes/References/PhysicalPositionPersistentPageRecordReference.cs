using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
    internal sealed class PhysicalPositionPersistentPageRecordReference : PageRecordReference
    {
        internal PhysicalPositionPersistentPageRecordReference(PageReference page, ushort physicalPosition) : base(page,physicalPosition)
        {

        }
        public override PageRecordReference Copy()
        {
            return new PhysicalPositionPersistentPageRecordReference(Page, PersistentRecordNum);
        }
    }

    internal sealed class NullPageRecordReference : PageRecordReference
    {
        internal NullPageRecordReference(PageReference page) : base(page, 0)
        {

        }
        public override PageRecordReference Copy()
        {
            return new NullPageRecordReference(Page);
        }

    }
}
