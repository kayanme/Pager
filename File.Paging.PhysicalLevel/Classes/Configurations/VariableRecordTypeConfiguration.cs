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
        internal override Type RecordType => typeof(TRecord);
        private readonly Func<TRecord, byte> _getRecordType;
        public byte GetRecordType(TRecord record) => _getRecordType(record);
        public Dictionary<byte, VariableSizeRecordDeclaration<TRecord>> RecordMap = new Dictionary<byte, VariableSizeRecordDeclaration<TRecord>>();
        public bool UseLogicalSlotInfo { get; }
        public VariableRecordTypePageConfiguration(Func<TRecord, byte> typeGet=null,bool useLogcalSlotInfo = false)
        {
            if (typeGet == null)
                _getRecordType = _ => 1;
            _getRecordType = typeGet;
            UseLogicalSlotInfo = useLogcalSlotInfo;
        }

        internal override IPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize,byte pageType)
        {
            return new ComplexRecordTypePage<TRecord>(headers, accessor, reference, pageSize,pageType, this);
        }

        internal override IPageHeaders CreateHeaders(IPageAccessor accessor,ushort shift)
        {

            return new VariableRecordPageHeaders(accessor.GetChildAccessorWithStartShift(shift),UseLogicalSlotInfo);
        }
    }

}
