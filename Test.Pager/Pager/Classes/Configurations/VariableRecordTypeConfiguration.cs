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
        private readonly Func<TRecord, byte> _getRecordType;
        public byte GetRecordType(TRecord record) => _getRecordType(record);
        internal Dictionary<byte, VariableSizeRecordDeclaration<TRecord>> RecordMap = new Dictionary<byte, VariableSizeRecordDeclaration<TRecord>>();

        public VariableRecordTypePageConfiguration(Func<TRecord, byte> typeGet=null)
        {
            if (typeGet == null)
                _getRecordType = _ => 1;
            _getRecordType = typeGet;
        }

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
