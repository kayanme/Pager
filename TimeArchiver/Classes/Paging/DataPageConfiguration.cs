using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using FIle.Paging.LogicalLevel.Classes.Configurations;
using TimeArchiver.Classes.Paging;


namespace TimeArchiver.Contracts
{
    internal class DataPageConfiguration : LogicalPageManagerConfiguration
    {



        private class DataPageGetter : IFixedSizeRecordDefinition<DataPageRecord<int>>
        {
            public int Size => 2*sizeof(ushort) + sizeof(int);

            public void FillBytes(ref DataPageRecord<int> record, byte[] targetArray)
            {
                RecordUtils.ToBytes(ref record.StampShift, targetArray, 0);
                RecordUtils.ToBytes(ref record.VersionShift, targetArray, sizeof(ushort));
                RecordUtils.ToBytes(ref record.Value, targetArray, 2*sizeof(ushort));
            }

            public void FillFromBytes(byte[] sourceArray, ref DataPageRecord<int> record)
            {
                RecordUtils.FromBytes(sourceArray, 0, ref record.StampShift);
                RecordUtils.FromBytes(sourceArray, sizeof(ushort), ref record.VersionShift);
                RecordUtils.FromBytes(sourceArray, 2*sizeof(ushort), ref record.Value);

            }
        }

        private class DataPageHeaderGetter : IHeaderDefinition<DataPageHeader>
        {
            public int Size => 2 * sizeof(long) + sizeof(ushort);

            public void FillBytes(ref DataPageHeader record, byte[] targetArray)
            {
                RecordUtils.ToBytes(ref record.StampOrigin, targetArray, 0);
                RecordUtils.ToBytes(ref record.VersionOrigin, targetArray, sizeof(long));
                RecordUtils.ToBytes(ref record.HasSameStampValues, targetArray, 2 * sizeof(long));
            }

            public void FillFromBytes(byte[] sourceArray, ref DataPageHeader record)
            {
                RecordUtils.FromBytes(sourceArray, 0, ref record.StampOrigin);
                RecordUtils.FromBytes(sourceArray, sizeof(long), ref record.VersionOrigin);
                RecordUtils.FromBytes(sourceArray, 2 * sizeof(long), ref record.HasSameStampValues);

            }
        }

        public DataPageConfiguration() : base(PageSize.Kb4)
        {
            DefinePageType(1).AsPageWithRecordType<DataPageRecord<int>>()                
                .UsingRecordDefinition(new DataPageGetter())
                .ApplyLogicalSortIndex()
                .WithHeader(new DataPageHeaderGetter())               
                .ApplyRecordOrdering(k=>k.StampShift);
        }
    }
}
