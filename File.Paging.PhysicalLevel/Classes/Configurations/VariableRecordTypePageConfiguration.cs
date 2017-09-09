using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace File.Paging.PhysicalLevel.Classes.Configurations
{
    internal sealed class VariableRecordTypePageConfiguration<TRecord> : PageContentConfiguration where TRecord:struct
    {
        internal override Type RecordType => typeof(TRecord);
      
      
        public VariableSizeRecordDeclaration<TRecord> RecordMap;
      

        public VariableRecordTypePageConfiguration()
        {        
            ConsistencyConfiguration = new ConsistencyConfiguration {ConsistencyAbilities = ConsistencyAbilities.None};

        }


        internal override HeaderInfo ReturnHeaderInfo()
        {
            return new HeaderInfo(false,WithLogicalSort,0);
        }

        public override void Verify()
        {
           
        }
    }

}
