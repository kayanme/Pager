using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using FIle.Paging.LogicalLevel.Classes.Configurations;

namespace TimeArchiver.Contracts
{
    internal class IndexRootConfiguration : LogicalPageManagerConfiguration
    {

        

        private class IndexRootGetter : IFixedSizeRecordDefinition<IndexRoot>
        {
            public int Size => sizeof(long) + RecordUtils.RecordReferenceLength;

            public void FillBytes(ref IndexRoot record, byte[] targetArray)
            {
                RecordUtils.ToBytes(ref record.TagNum, targetArray, 0);
                RecordUtils.ToBytes(ref record.Root, targetArray, sizeof(long));
            }

            public void FillFromBytes(byte[] sourceArray, ref IndexRoot record)
            {
                RecordUtils.FromBytes(sourceArray, 0, ref record.TagNum);
                RecordUtils.FromBytes(sourceArray, sizeof(long), ref record.Root);

            }
        }


        public IndexRootConfiguration() : base(PageSize.Kb8)
        {            
            DefinePageType(1).AsPageWithRecordType<IndexRoot>().UsingRecordDefinition(new IndexRootGetter()).AsVirtualHeapPage(2);
        }
    }
}
