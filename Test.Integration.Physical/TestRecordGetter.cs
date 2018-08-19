using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Configuration.Builder;


namespace Test.Integration.Physical
{
    public class TestRecordGetter : IFixedSizeRecordDefinition<TestRecord>
    {
        public  void FillBytes(ref TestRecord record,byte[] targetArray)
        {
           RecordUtils.ToBytes(ref record.Value, targetArray,0);

        }

        public  void FillFromBytes(byte[] sourceArray,ref TestRecord record)
        {

            RecordUtils.FromBytes(sourceArray, 0, ref record.Value);

        }

        public int Size => sizeof(long);
    }
}