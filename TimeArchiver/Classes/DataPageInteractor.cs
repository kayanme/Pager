using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimeArchiver.Classes.Paging;
using TimeArchiver.Contracts;

namespace TimeArchiver.Classes
{
    internal sealed class DataPageInteractor<T> : IDataPageInteractor<T> where T : struct
    {
        private readonly byte _pageType;
        private readonly IPageManager _pageManager;

        public DataPageInteractor(byte pageType, IPageManager pageManager)
        {
            this._pageType = pageType;
            this._pageManager = pageManager;
        }

        public DataPageRef CreateDataBlock(DataRecord<T>[] records)
        {
            var minStamp = records.Min(k => k.Stamp);
            var maxStamp = records.Max(k => k.Stamp);
            Debug.Assert(maxStamp - minStamp < ushort.MaxValue, "maxStamp - minStamp < ushort.MaxValue");
            var minVersion = records.Min(k => k.VersionStamp);
            var maxVersion = records.Max(k => k.VersionStamp);
            Debug.Assert(maxVersion - minVersion < ushort.MaxValue, "maxVersion - minVersion < ushort.MaxValue");

            var hasSameStampValues = records.Select(k => k.Stamp).Distinct().Count() != records.Count() ? (ushort)1 : (ushort)0;


            var page = _pageManager.CreatePage(_pageType);
            var head = _pageManager.GetHeaderAccessor<DataPageHeader>(page);
            head.ModifyHeader(new DataPageHeader { StampOrigin = minStamp, VersionOrigin = minVersion, HasSameStampValues = hasSameStampValues });

            using (var accessor = _pageManager.GetRecordAccessor<DataPageRecord<T>>(page) )
            {
                foreach (var record in records)
                {
                    accessor.AddRecord(new DataPageRecord<T>
                    {
                        Value = record.Data,
                        StampShift = (ushort)(record.Stamp - minStamp),
                        VersionShift = (ushort)(record.VersionStamp - minVersion)
                    });
                }
                accessor.Flush();
            }
            return new DataPageRef { Start = minStamp, End = maxStamp, DataReference = page };
        }

        public DataRecord<T> FindClosestLeft(DataPageRef dataPage, long stamp)
        {
            Debug.Assert(dataPage.Start <= stamp && dataPage.End >= stamp, "dataPage.Start <= stamp && dataPage.End >= stamp");
            if (dataPage.Start > stamp || dataPage.End < stamp)
                throw new ArgumentException(nameof(stamp));
            var head = _pageManager.GetHeaderAccessor<DataPageHeader>(dataPage.DataReference).GetHeader();
            var shift = stamp - head.StampOrigin;
            DataPageRecord<T> data;
            using (var accessor = _pageManager.GetRecordAccessor<DataPageRecord<T>>(dataPage.DataReference) as IOrderedPage<DataPageRecord<T>, long>)
            {
                data = accessor.FindTheLessGreater(stamp, true).Data;                

            }
            return new DataRecord<T>
            {
                Stamp = data.StampShift + head.StampOrigin,
                VersionStamp = data.VersionShift + head.VersionOrigin,
                Data = data.Value
            };
        }

        public DataRecord<T>[] FindRange(DataPageRef dataPage, long start, long end)
        {
            var head = _pageManager.GetHeaderAccessor<DataPageHeader>(dataPage.DataReference).GetHeader();
            var leftShift = start - head.StampOrigin;
            var rightShift = end - head.StampOrigin;
            using (var accessor = _pageManager.GetBinarySearchForPage<DataPageRecord<T>>(dataPage.DataReference) as IOrderedPage<DataPageRecord<T>, long>)
            {
                var data = accessor.FindInKeyRange(start, end).Select(k => new DataRecord<T>
                {
                    Stamp = k.Data.StampShift + head.StampOrigin,
                    VersionStamp = k.Data.VersionShift + head.VersionOrigin,
                    Data = k.Data.Value
                });
                                      

                if (head.HasSameStampValues != 0)
                    data = data.GroupBy(k => k.Stamp).Select(k => k.OrderBy(k2 => k2.VersionStamp).Last());
                return data.ToArray();
            }
        }
    }
}
