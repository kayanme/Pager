using System;
using System.Linq;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace Durability.Paging.PhysicalLevel
{
    internal class Proc1 : IVariableSizeRecordDefinition<TestRecord>
    {
        public void FillBytes(TestRecord r, byte[] b)
        {
            Buffer.BlockCopy(r.Data.ToByteArray(), 0, b, 0, b.Count());
          
        }

        public void FillFromBytes(byte[] b, TestRecord r)
        {
            r.Data = new Guid(b.ToArray());
        }

        public int Size(TestRecord record)
        {
            return 16;
        }
    }
}