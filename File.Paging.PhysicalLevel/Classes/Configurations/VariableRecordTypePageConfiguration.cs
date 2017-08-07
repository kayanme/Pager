using System;
using System.Collections.Generic;
using System.Linq;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal sealed class VariableRecordTypePageConfiguration<TRecord> : PageContentConfiguration where TRecord:TypedRecord,new()
    {
        internal override Type RecordType => typeof(TRecord);
        private readonly Func<TRecord, byte> _getRecordType;
        public byte GetRecordType(TRecord record) => _getRecordType(record);
        public Dictionary<byte, VariableSizeRecordDeclaration<TRecord>> RecordMap = new Dictionary<byte, VariableSizeRecordDeclaration<TRecord>>();
        public bool UseLogicalSlotInfo { get; set; }

        public VariableRecordTypePageConfiguration(Func<TRecord, byte> typeGet = null)
        {
            if (typeGet == null)
                _getRecordType = _ => 1;
            _getRecordType = typeGet;

            ConsistencyConfiguration = new ConsistencyConfiguration {ConsistencyAbilities = ConsistencyAbilities.None};

        }



        internal override IPage CreatePage(IPageHeaders headers, IPageAccessor accessor, PageReference reference, int pageSize,byte pageType)
        {
            return new ComplexRecordTypePage<TRecord>(headers, accessor, reference, pageSize,pageType, this);
        }

        internal override IPageHeaders CreateHeaders(IPageAccessor accessor,ushort shift)
        {

            return new VariableRecordPageHeaders(accessor.GetChildAccessorWithStartShift(shift),UseLogicalSlotInfo);
        }

        public override void Verify()
        {
            if (!RecordMap.Any())
                throw new ArgumentException($"No record definitions");
        }
    }

}
