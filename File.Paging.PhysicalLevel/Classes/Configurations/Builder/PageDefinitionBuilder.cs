using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    internal class PageDefinitionBuilder<TRecordType> : PageDefinitionBuilder,
        IPageRecordTypeBuilder<TRecordType>,
        IFixedPageBuilder<TRecordType>,
        IVariablePageBuilder<TRecordType>,
        IVariablePageWithOneRecordTypeBuilder,
        IHeaderedFixedPageBuilder<TRecordType>,
        IHeaderedVariablePageWithOneRecordBuilder,
        IHeaderedVariablePageBuilder<TRecordType> where TRecordType : TypedRecord, new()
    {
      
        public PageDefinitionBuilder(PageManagerConfiguration config, byte pageNum):base(config,pageNum)
        {
        }

        public IFixedPageBuilder<TRecordType> UsingRecordDefinition(IFixedSizeRecordDefinition<TRecordType> recordDefinition)
        {
            if (recordDefinition == null) throw new ArgumentNullException(nameof(recordDefinition));
            var conf = new FixedRecordTypePageConfiguration<TRecordType>();
            conf.RecordMap = new FixedSizeRecordDeclaration<TRecordType>(recordDefinition.FillBytes,recordDefinition.FillFromBytes,recordDefinition.Size);
            _config.PageMap[_pageNum] = conf;
            return this;
        }

        public IFixedPageBuilder<TRecordType> UsingRecordDefinition(Action<TRecordType,byte[]> fillBytes,Action<byte[],TRecordType> fillFromBytes,int size)
        {
            if (fillBytes == null) throw new ArgumentNullException(nameof(fillBytes));
            if (fillFromBytes == null) throw new ArgumentNullException(nameof(fillFromBytes));

            var conf = new FixedRecordTypePageConfiguration<TRecordType>();
            conf.RecordMap = new FixedSizeRecordDeclaration<TRecordType>(fillBytes, fillFromBytes, size);
            _config.PageMap[_pageNum] =  conf;
            return this;
        }

        public IVariablePageBuilder<TRecordType> WithMultipleTypeRecord(Func<TRecordType, byte> discriminatorFunction)
        {
            var conf = new VariableRecordTypePageConfiguration<TRecordType>(discriminatorFunction);
            _config.PageMap[_pageNum] = conf;
            return this;
        }

        public IVariablePageWithOneRecordTypeBuilder UsingRecordDefinition(Action<TRecordType, byte[]> fillBytes, Action<byte[], TRecordType> fillFromBytes, Func<TRecordType,int> size)
        {
            if (fillBytes == null) throw new ArgumentNullException(nameof(fillBytes));
            if (fillFromBytes == null) throw new ArgumentNullException(nameof(fillFromBytes));
            if (size == null) throw new ArgumentNullException(nameof(size));
            var config = new VariableRecordTypePageConfiguration<TRecordType>();
            _config.PageMap[_pageNum] = config;
            config.RecordMap.Add(1, new VariableSizeRecordDeclaration<TRecordType>(fillBytes, fillFromBytes, size));
            return this;
        }

        public IVariablePageWithOneRecordTypeBuilder UsingRecordDefinition(IVariableSizeRecordDefinition<TRecordType> recordDefinition)
        {
            if (recordDefinition == null) throw new ArgumentNullException(nameof(recordDefinition));
            var config = new VariableRecordTypePageConfiguration<TRecordType>();
            _config.PageMap[_pageNum] = config;
            config.RecordMap.Add(1, new VariableSizeRecordDeclaration<TRecordType>(recordDefinition.FillBytes, recordDefinition.FillFromBytes, recordDefinition.Size));
            return this;
        }

        public IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType,IVariableSizeRecordDefinition<TRecordType> recordDefinition)
        {
            if (recordDefinition == null) throw new ArgumentNullException(nameof(recordDefinition));
            var config = _config.PageMap[_pageNum] as VariableRecordTypePageConfiguration<TRecordType>;
            config.RecordMap.Add(recordType,new VariableSizeRecordDeclaration<TRecordType>(recordDefinition.FillBytes,recordDefinition.FillFromBytes,recordDefinition.Size));
            return this;
        }

        public IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType, Action<TRecordType, byte[]> fillBytes, Action<byte[], TRecordType> fillFromBytes, Func<TRecordType, int> size)
        {
            if (fillBytes == null) throw new ArgumentNullException(nameof(fillBytes));
            if (fillFromBytes == null) throw new ArgumentNullException(nameof(fillFromBytes));
            if (size == null) throw new ArgumentNullException(nameof(size));

            var config = _config.PageMap[_pageNum] as VariableRecordTypePageConfiguration<TRecordType>;
            config.RecordMap.Add(recordType, new VariableSizeRecordDeclaration<TRecordType>(fillBytes, fillFromBytes, size));
            return this;
        }



        public IVariablePageBuilder<TRecordType> UsingRecordDefinition(byte recordType, IFixedSizeRecordDefinition<TRecordType> recordDefinition)
        {
            if (recordDefinition == null) throw new ArgumentNullException(nameof(recordDefinition));
            var config = _config.PageMap[_pageNum] as VariableRecordTypePageConfiguration<TRecordType>;
            config.RecordMap.Add(recordType, new VariableSizeRecordDeclaration<TRecordType>(recordDefinition.FillBytes, recordDefinition.FillFromBytes,_=>recordDefinition.Size));
            return Copy();
        }

        IVariablePageBuilder<TRecordType> IHeaderedVariablePageBuilder<TRecordType>.ApplyLogicalSortIndex()
        {
           return ApplyLogicalSort();
        }

        IVariablePageBuilder<TRecordType> IHeaderedVariablePageBuilder<TRecordType>.WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities)
        {
            return WithConsistencyAbilities(consitencyAbilities);
        }

        IVariablePageBuilder<TRecordType> IVariablePageBuilder<TRecordType>.ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort();
        }

        IVariablePageWithOneRecordTypeBuilder IVariablePageWithOneRecordTypeBuilder.ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort();
        }

        private PageDefinitionBuilder<TRecordType> ApplyLogicalSort()
        {
            var config = _config.PageMap[_pageNum] as VariableRecordTypePageConfiguration<TRecordType>;
            config.UseLogicalSlotInfo = true;
            return this;
        }

        private PageDefinitionBuilder<TRecordType> WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities)
        {
            var c = _config.PageMap[_pageNum];
            c.ConsistencyConfiguration = new ConsistencyConfiguration(){ConsistencyAbilities = consitencyAbilities};
            return this;
        }

        IHeaderedVariablePageWithOneRecordBuilder IVariablePageWithOneRecordTypeBuilder.WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition)
        {
            return CreateHeaderedConfiguration(headerDefinition);
        }

        IHeaderedVariablePageBuilder<TRecordType> IVariablePageBuilder<TRecordType>.WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition)
        {
            return CreateHeaderedConfiguration(headerDefinition);
        }

        IHeaderedFixedPageBuilder<TRecordType> IFixedPageBuilder<TRecordType>.WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition)
        {
            return CreateHeaderedConfiguration(headerDefinition);
        }

        private PageDefinitionBuilder<TRecordType> CreateHeaderedConfiguration<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader:new()
        {
            var c = _config.PageMap[_pageNum];
            var c2 = new PageHeadersConfiguration<THeader>();
            c2.Header = new FixedSizeRecordDeclaration<THeader>(headerDefinition.FillBytes, headerDefinition.FillFromBytes,
                headerDefinition.Size);
            c2.InnerPageMap = c;
            _config.HeaderConfig.Add(_pageNum, c2);
            return this;
        }

        private PageDefinitionBuilder<TRecordType> Copy()
        {
            return new PageDefinitionBuilder<TRecordType>(_config, _pageNum);
        }

        IFixedPageBuilder<TRecordType> IFixedPageBuilder<TRecordType>.WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities)
        {
            return WithConsistencyAbilities(consitencyAbilities);
        }       

        IVariablePageBuilder<TRecordType> IVariablePageBuilder<TRecordType>.WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities)
        {
            return WithConsistencyAbilities(consitencyAbilities);
        }

        IVariablePageWithOneRecordTypeBuilder IVariablePageWithOneRecordTypeBuilder.WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities)
        {
            return WithConsistencyAbilities(consitencyAbilities);
        }

        IHeaderedFixedPageBuilder<TRecordType> IHeaderedFixedPageBuilder<TRecordType>.WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities)
        {
            return WithConsistencyAbilities(consitencyAbilities);
        }

        public IVariablePageWithOneRecordTypeBuilder ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort();
        }

        IVariablePageWithOneRecordTypeBuilder IHeaderedVariablePageWithOneRecordBuilder.WithConsistencyAbilities(ConsistencyAbilities consitencyAbilities)
        {
            return WithConsistencyAbilities(consitencyAbilities);
        }
    }
}