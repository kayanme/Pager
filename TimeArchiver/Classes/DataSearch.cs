using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using FIle.Paging.LogicalLevel.Classes.Configurations;
using FIle.Paging.LogicalLevel.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TimeArchiver.Classes;

namespace TimeArchiver.Contracts
{
    internal sealed class DataSearch : IDataSearch
    {
        private readonly IPage<IndexRoot> _indexRootPage;
        private readonly IPage<IndexPageRecord> _indexPage1;
        private readonly IPage<IndexPageRecord> _indexPage2;
        private IDataPageInteractor<int> _intDataPages;
        private ConcurrentDictionary<long, IDataInteraction<int>> _intTagCache;

        private List<IDisposable> _disposablesToClear = new List<IDisposable>();

        public DataSearch(string indexRootFilename, string indexFile1, string indexFile2, string dataFile, ILogicalPageManagerFactory pageManagerFactory)
        {

            _indexRootPage = CreateVirtualPage<IndexRoot, IndexRootConfiguration>(pageManagerFactory, indexRootFilename);
            _indexPage1 = CreateVirtualPage<IndexPageRecord, IndexFileConfiguration>(pageManagerFactory, indexFile1);
            _indexPage2 = CreateVirtualPage<IndexPageRecord, IndexFileConfiguration>(pageManagerFactory, indexFile2);
            var dataPage = pageManagerFactory.CreateManager(dataFile, new DataPageConfiguration(), true);
            _intDataPages = new DataPageInteractor<int>(1, dataPage);
            _intTagCache = new ConcurrentDictionary<long, IDataInteraction<int>>(
                _indexRootPage.IterateRecords()
                .ToDictionary(k => k.Data.TagNum, k => CreateIntInteractor(k.Data.Root)));
        }
        private IPage<T> CreateVirtualPage<T, C>(ILogicalPageManagerFactory pageManagerFactory, string filename) where C : LogicalPageManagerConfiguration, new() where T : struct
        {
            var rootManager = pageManagerFactory.CreateManager(filename, new C(), true);
            _disposablesToClear.Add(rootManager);
            return rootManager.GetRecordAccessor<T>(rootManager.CreatePage(1));
        }

        public async Task CreateTag(long num, TagType type)
        {
            var indexInteractor = new IndexInteractor(_indexPage1, _indexPage2);
            await indexInteractor.PrepareIndexChange();
            var rootReference = indexInteractor.InitializeRoot();
            await indexInteractor.FinalizeIndexChange();
            var rootRec = new IndexRoot { TageType = (byte)type, TagNum = num, Root = rootReference };
            var rec = _indexRootPage.AddRecord(rootRec);
            switch (type)
            {
                case TagType.Int:
                    if (!_intTagCache.TryAdd(rootRec.TagNum, CreateIntInteractor(rootReference)))
                        _indexRootPage.FreeRecord(rec);
                    break;
                default: throw new NotSupportedException();
            }
        }

        public IAsyncEnumerable<DataRecord<int>[]> FindInRangeInt(long tag, long start, long end) 
        {
            if (!_intTagCache.TryGetValue(tag, out var dataSearch))
                throw new InvalidOperationException($"Tag {tag} not found");
            return dataSearch.FindInRange(start, end);
        }

        private IDataInteraction<int> CreateIntInteractor(PageRecordReference rootIndex)
        {
            var interactor = new IndexInteractor(_indexPage1, _indexPage2, rootIndex);
            var dataSearch = new DataInteraction<int>(interactor, interactor, _intDataPages);
            return dataSearch;
        }

        public async Task InsertBlock(long tag, DataRecord<int>[] block)
        {

            if (!_intTagCache.TryGetValue(tag, out var dataSearch))
                throw new InvalidOperationException($"Tag {tag} not found");

            await dataSearch.AddBlock(block);
        }

        #region IDisposable Support
        private bool disposedValue = false;


        public void Dispose()
        {
            if (!disposedValue)
            {

                _indexPage1.Dispose();
                _indexPage2.Dispose();
                _indexRootPage.Dispose();
                _intDataPages.Dispose();
                foreach(var d in _disposablesToClear)
                {
                    d.Dispose();
                }

                disposedValue = true;
            }
        }
        #endregion

    }


}
