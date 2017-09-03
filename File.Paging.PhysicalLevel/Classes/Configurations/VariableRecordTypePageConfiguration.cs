using System;
using System.Collections.Generic;
using System.Diagnostics;
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
      

        public VariableRecordTypePageConfiguration(Func<TRecord, byte> typeGet = null)
        {
            if (typeGet == null)
                _getRecordType = _ => 1;
            else
               _getRecordType = typeGet;         
            ConsistencyConfiguration = new ConsistencyConfiguration {ConsistencyAbilities = ConsistencyAbilities.None};

        }


        //internal override IPageHeaders CreateHeaders(IPageAccessor accessor,ushort shift)
        //{

        //    return WithLogicalSort?
        //          (IPageHeaders)new VariableRecordWithLogicalOrderHeaders(accessor.GetChildAccessorWithStartShift(shift))
        //                      : new VariableRecordPageHeaders(accessor.GetChildAccessorWithStartShift(shift));
        //}

        internal override HeaderInfo ReturnHeaderInfo()
        {
            return new HeaderInfo(false,WithLogicalSort,0);
        }

        public override void Verify()
        {
            if (!RecordMap.Any())
                throw new ArgumentException($"No record definitions");
        }
    }

}
