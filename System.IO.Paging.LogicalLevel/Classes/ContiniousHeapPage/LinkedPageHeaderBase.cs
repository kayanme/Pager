using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration.Builder;

namespace System.IO.Paging.LogicalLevel.Classes.ContiniousHeapPage
{
    public class LinkedPageHeaderBase: IHeaderDefinition<LinkedPageHeaderBase>
    {
        public PageReference PreviousPage { get; set; }
        public PageReference NextPage { get; set; }

        public void FillBytes(ref LinkedPageHeaderBase record, byte[] targetArray)
        {
            throw new NotImplementedException();
        }

        public void FillFromBytes(byte[] sourceArray,ref LinkedPageHeaderBase record)
        {
            throw new NotImplementedException();
        }

        public int Size { get; }
    }
}
