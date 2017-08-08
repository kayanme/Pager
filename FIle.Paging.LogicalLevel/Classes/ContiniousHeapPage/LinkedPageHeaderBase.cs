using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace FIle.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
    public class LinkedPageHeaderBase: IHeaderDefinition<LinkedPageHeaderBase>
    {
        public PageReference PreviousPage { get; set; }
        public PageReference NextPage { get; set; }

        public void FillBytes(LinkedPageHeaderBase record, byte[] targetArray)
        {
            throw new NotImplementedException();
        }

        public void FillFromBytes(byte[] sourceArray, LinkedPageHeaderBase record)
        {
            throw new NotImplementedException();
        }

        public int Size { get; }
    }
}
