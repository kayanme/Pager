using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Configuration.Builder;

namespace Test.Integration.Physical
{
    public class TestHeader
    {
        public string HeaderInfo;
    }

    public class TestHeaderGetter : IHeaderDefinition<TestHeader>
    {
        public  void FillBytes(ref TestHeader record, byte[] targetArray)
        {
            
           RecordUtils.StringToBytes(targetArray,0,record.HeaderInfo,32);
           
        }

        public  void FillFromBytes(byte[] sourceArray,ref TestHeader record)
        {
            record.HeaderInfo = RecordUtils.StringFromBytes(sourceArray, 0,  32);
        }

        public int Size => sizeof(char)*32;
    }
}
