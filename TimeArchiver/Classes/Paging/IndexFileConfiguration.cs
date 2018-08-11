using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using FIle.Paging.LogicalLevel.Classes.Configurations;

namespace TimeArchiver.Contracts
{
    internal class IndexFileConfiguration : LogicalPageManagerConfiguration
    {

        private class IndexGetter : IFixedSizeRecordDefinition<IndexPageRecord>
        {
            public int Size => sizeof(long)*2 + sizeof(short) +  2* RecordUtils.RecordReferenceLength;

            public void FillBytes(ref IndexPageRecord record, byte[] targetArray)
            {            
                RecordUtils.ToBytes(ref record.Start, targetArray, 0);
                RecordUtils.ToBytes(ref record.End, targetArray, sizeof(long));
                if (record.ChildrenOne == null && record.Data == null)
                    record.MaxUnderlyingDepth = -1;
                else if (record.Data != null)
                    record.MaxUnderlyingDepth = 0;
                else
                    record.MaxUnderlyingDepth = 1;
                RecordUtils.ToBytes(ref record.MaxUnderlyingDepth, targetArray, sizeof(long)*2);

                if (record.ChildrenOne != null)
                    RecordUtils.ToBytes(ref record.ChildrenOne, targetArray, sizeof(long) * 2 + sizeof(short));
                else if (record.Data != null)
                {
                    RecordUtils.ToBytes(ref record.Data, targetArray, sizeof(long) * 2 + sizeof(short));
                }

                if (record.ChildrenTwo != null)
                    RecordUtils.ToBytes(ref record.ChildrenTwo, targetArray, sizeof(long) * 2 + sizeof(short) + RecordUtils.RecordReferenceLength);

            }

            public void FillFromBytes(byte[] sourceArray, ref IndexPageRecord record)
            {
                RecordUtils.FromBytes(sourceArray, 0, ref record.Start);
                RecordUtils.FromBytes(sourceArray, sizeof(long), ref record.End);
                RecordUtils.FromBytes(sourceArray, sizeof(long) * 2, ref record.MaxUnderlyingDepth);
                if (record.MaxUnderlyingDepth == 0)                
                    RecordUtils.FromBytes(sourceArray, sizeof(long) * 2 + sizeof(short), ref record.Data);                
                else if (record.MaxUnderlyingDepth>0)
                {
                    RecordUtils.FromBytes(sourceArray, sizeof(long) * 2 + sizeof(short), ref record.ChildrenOne);
                    RecordUtils.FromBytes(sourceArray, sizeof(long) * 2 + sizeof(short) + RecordUtils.RecordReferenceLength, ref record.ChildrenTwo);
                }
                                  
            }
        }


        public IndexFileConfiguration() : base(PageSize.Kb8)
        {
            DefinePageType(1).AsPageWithRecordType<IndexPageRecord>().UsingRecordDefinition(new IndexGetter()).AsVirtualHeapPage(2);
        }
    }
}
