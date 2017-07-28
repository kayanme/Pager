using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pager.Contracts;
using Pager.Implementations;

namespace Pager.Classes
{
    public class VariableRecordTypePageConfiguration<TRecord> : PageConfiguration where TRecord:TypedRecord,new()
    {
      
        public Dictionary<byte, VariableSizeRecordDeclaration<TRecord>> RecordMap = new Dictionary<byte, VariableSizeRecordDeclaration<TRecord>>();

        internal override TypedPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize)
        {
            return new ComplexRecordTypePage<TRecord>(headers, accessor, reference, pageSize, this);
        }

        internal override IPageHeaders CreateHeaders(IPageAccessor accessor,ushort shift)
        {
            return new VariableRecordPageHeaders(accessor,0);
        }
    }

}
