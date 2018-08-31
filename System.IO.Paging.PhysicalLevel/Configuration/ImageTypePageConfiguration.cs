using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Paging.PhysicalLevel.Configuration
{
    internal sealed class ImageTypePageConfiguration<TRecordType> : FixedRecordTypePageConfiguration<TRecordType> where TRecordType : new()
    {
        public ImageTypePageConfiguration()
        {
            ConsistencyConfiguration = new ConsistencyConfiguration {ConsistencyAbilities = ConsistencyAbilities.None };
        }
        
        internal override Type RecordType => typeof(TRecordType);

        
        public override void Verify()
        {
            if (RecordMap.GetSize != PageSize)
            {
                throw new ArgumentException($"Record length for image pages should be of a page size ({PageSize})");
            }
        }

        internal override HeaderInfo ReturnHeaderInfo()
        {
            return new HeaderInfo(true,false,PageSize);
        }
    }
}
