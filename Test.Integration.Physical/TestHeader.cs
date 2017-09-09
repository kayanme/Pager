using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

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
