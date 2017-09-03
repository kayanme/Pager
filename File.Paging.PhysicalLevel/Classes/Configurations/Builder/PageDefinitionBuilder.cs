using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    internal class PageDefinitionBuilder<TRecordType, THeader> : PageDefinitionBuilder<TRecordType>,
        IHeaderedFixedPageBuilder<TRecordType, THeader>,
        IHeaderedVariablePageWithOneRecordBuilder<TRecordType, THeader>,
        IHeaderedVariablePageBuilder<TRecordType, THeader> where TRecordType : TypedRecord, new() where THeader:new()
    {
        IHeaderedVariablePageBuilder<TRecordType, THeader> IHeaderedVariablePageBuilder<TRecordType, THeader>.ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort() as PageDefinitionBuilder<TRecordType, THeader>;
        }

        IHeaderedVariablePageWithOneRecordBuilder<TRecordType, THeader> IHeaderedVariablePageWithOneRecordBuilder<TRecordType, THeader>.ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort() as PageDefinitionBuilder<TRecordType, THeader>;
        }

        IHeaderedVariablePageBuilder<TRecordType, THeader> IHeaderedVariablePageBuilder<TRecordType, THeader>.ApplyLockScheme(LockRuleset locksRuleset)
        { 
            return ApplyLockScheme(locksRuleset) as PageDefinitionBuilder<TRecordType, THeader>;
        }

        IHeaderedVariablePageWithOneRecordBuilder<TRecordType, THeader> IHeaderedVariablePageWithOneRecordBuilder<TRecordType, THeader>.ApplyLockScheme(LockRuleset locksRuleset)
        {
            return ApplyLockScheme(locksRuleset) as PageDefinitionBuilder<TRecordType, THeader>;
        }

        IHeaderedFixedPageBuilder<TRecordType, THeader> IHeaderedFixedPageBuilder<TRecordType, THeader>.ApplyLockScheme(LockRuleset locksRuleset)
        {
            return ApplyLockScheme(locksRuleset) as PageDefinitionBuilder<TRecordType, THeader>;
        }

        public PageDefinitionBuilder(PageManagerConfiguration config, byte pageNum) : base(config, pageNum)
        {
        }
    }

    internal class PageDefinitionBuilder<TRecordType> : PageDefinitionBuilder,
        IPageRecordTypeBuilder<TRecordType>,
        IFixedPageBuilder<TRecordType>,
        IVariablePageBuilder<TRecordType>,
        IVariablePageWithOneRecordTypeBuilder<TRecordType>
         where TRecordType : TypedRecord, new()
    {
      
        public PageDefinitionBuilder(PageManagerConfiguration config, byte pageNum):base(config,pageNum)
        {
        }

        public IFixedPageBuilder<TRecordType> UsingRecordDefinition(IFixedSizeRecordDefinition<TRecordType> recordDefinition)
        {
            if (recordDefinition == null) throw new ArgumentNullException(nameof(recordDefinition));
            var conf = new FixedRecordTypePageConfiguration<TRecordType>();
            conf.PageSize = Config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? (ushort)4096 : (ushort)8192;
            conf.RecordMap = new FixedSizeRecordDeclaration<TRecordType>(recordDefinition.FillBytes,recordDefinition.FillFromBytes,recordDefinition.Size);
            _config.PageMap[_pageNum] = conf;
            return this;
        }

        public IFixedPageBuilder<TRecordType> UsingRecordDefinition(Action<TRecordType,byte[]> fillBytes,Action<byte[],TRecordType> fillFromBytes,int size)
        {
            if (fillBytes == null) throw new ArgumentNullException(nameof(fillBytes));
            if (fillFromBytes == null) throw new ArgumentNullException(nameof(fillFromBytes));

            var conf = new FixedRecordTypePageConfiguration<TRecordType>();
            conf.PageSize = Config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? (ushort)4096 : (ushort)8192;
            conf.RecordMap = new FixedSizeRecordDeclaration<TRecordType>(fillBytes, fillFromBytes, size);
            _config.PageMap[_pageNum] =  conf;
            return this;
        }

        public IVariablePageBuilder<TRecordType> WithMultipleTypeRecord(Func<TRecordType, byte> discriminatorFunction)
        {
            var conf = new VariableRecordTypePageConfiguration<TRecordType>(discriminatorFunction);
            conf.PageSize = Config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? (ushort)4096 : (ushort)8192;
            _config.PageMap[_pageNum] = conf;
            return this;
        }

        public IVariablePageWithOneRecordTypeBuilder<TRecordType> UsingRecordDefinition(Action<TRecordType, byte[]> fillBytes, Action<byte[], TRecordType> fillFromBytes, Func<TRecordType,int> size)
        {
            if (fillBytes == null) throw new ArgumentNullException(nameof(fillBytes));
            if (fillFromBytes == null) throw new ArgumentNullException(nameof(fillFromBytes));
            if (size == null) throw new ArgumentNullException(nameof(size));
            var config = new VariableRecordTypePageConfiguration<TRecordType>();
            config.PageSize = Config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? (ushort)4096 : (ushort)8192;
            _config.PageMap[_pageNum] = config;
            config.RecordMap.Add(1, new VariableSizeRecordDeclaration<TRecordType>(fillBytes, fillFromBytes, size));
            return this;
        }

        public IVariablePageWithOneRecordTypeBuilder<TRecordType> UsingRecordDefinition(IVariableSizeRecordDefinition<TRecordType> recordDefinition)
        {
            if (recordDefinition == null) throw new ArgumentNullException(nameof(recordDefinition));
            var config = new VariableRecordTypePageConfiguration<TRecordType>();
            config.PageSize = Config.SizeOfPage == PageManagerConfiguration.PageSize.Kb4 ? (ushort)4096 : (ushort)8192;
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

      

        IVariablePageBuilder<TRecordType> IVariablePageBuilder<TRecordType>.ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort();
        }

        IVariablePageWithOneRecordTypeBuilder<TRecordType> IVariablePageWithOneRecordTypeBuilder<TRecordType>.ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort();
        }



         IFixedPageBuilder<TRecordType> IFixedPageBuilder<TRecordType>.ApplyLogicalSortIndex()
        {
            return ApplyLogicalSort();
        }

        protected PageDefinitionBuilder<TRecordType> ApplyLogicalSort()
        {
            var config = _config.PageMap[_pageNum];
            config.WithLogicalSort = true;
            return this;
        }

        protected PageDefinitionBuilder<TRecordType> ApplyLockScheme(LockRuleset lockRules)
        {
            var c = _config.PageMap[_pageNum];
            c.ConsistencyConfiguration = new ConsistencyConfiguration()
            {
                ConsistencyAbilities = ConsistencyAbilities.PhysicalLocks,LockRules = lockRules
            };
            return this;
        }

       

        private PageDefinitionBuilder<TRecordType, THeader> CreateHeaderedConfiguration<THeader>(IHeaderDefinition<THeader> headerDefinition) where THeader:new()
        {
            var c = _config.PageMap[_pageNum];
            var c2 = new PageHeadersConfiguration<THeader>();
            c2.Header = new FixedSizeRecordDeclaration<THeader>(headerDefinition.FillBytes, headerDefinition.FillFromBytes,
                headerDefinition.Size);
            c2.InnerPageMap = c;
            _config.HeaderConfig.Add(_pageNum, c2);
            return new PageDefinitionBuilder<TRecordType,THeader>(_config,_pageNum);
        }

        private PageDefinitionBuilder<TRecordType> Copy()
        {
            return new PageDefinitionBuilder<TRecordType>(_config, _pageNum);
        }

        IFixedPageBuilder<TRecordType> IFixedPageBuilder<TRecordType>.ApplyLockScheme(LockRuleset consitencyAbilities)
        {
            return ApplyLockScheme(consitencyAbilities);
        }       

        IVariablePageBuilder<TRecordType> IVariablePageBuilder<TRecordType>.ApplyLockScheme(LockRuleset consitencyAbilities)
        {
            return ApplyLockScheme(consitencyAbilities);
        }

        IVariablePageWithOneRecordTypeBuilder<TRecordType> IVariablePageWithOneRecordTypeBuilder<TRecordType>.ApplyLockScheme(LockRuleset consitencyAbilities)
        {
            return ApplyLockScheme(consitencyAbilities);
        }

        IHeaderedVariablePageWithOneRecordBuilder<TRecordType, THeader> IVariablePageWithOneRecordTypeBuilder<TRecordType>.WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition) 
        {
            return CreateHeaderedConfiguration(headerDefinition);
        }

        IHeaderedVariablePageBuilder<TRecordType, THeader> IVariablePageBuilder<TRecordType>.WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition)
        {
            return CreateHeaderedConfiguration(headerDefinition);
        }

        IHeaderedFixedPageBuilder<TRecordType,THeader> IFixedPageBuilder<TRecordType>.WithHeader<THeader>(IHeaderDefinition<THeader> headerDefinition)
        {
            return CreateHeaderedConfiguration(headerDefinition);
        }

     
    }
}