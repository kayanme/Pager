using System.IO.Paging.LogicalLevel.Classes.ContiniousHeapPage;
using System.IO.Paging.LogicalLevel.Classes.OrderedPage;
using System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements;

namespace System.IO.Paging.LogicalLevel.Configuration
{
    public static class ExtensionBuilder
    {
        private static void AddOrderingCommon<TRecord, TKey>(Func<TRecord, TKey> keySelector, PageDefinitionBuilder t)
            where TRecord : struct where TKey : IComparable<TKey>
        {
            if (!(t.Config is LogicalPageManagerConfiguration))
                throw new ArgumentException("Should build upon LogicalPageManagerConfiguration");
            var e = t.Config as LogicalPageManagerConfiguration;
            if (e.Configuration.ContainsKey(t.PageNum))
                throw new InvalidOperationException($"Page {t.PageNum} has logic defined already");
            e.Configuration.Add(t.PageNum, new OrderedLogicalPageConfiguration<TRecord, TKey>() { KeySelector = keySelector });
        }

        public static void ApplyRecordOrdering<TRecord, THeader, TKey>(this IHeaderedFixedPageBuilder<TRecord, THeader> builder,Func<TRecord,TKey> keySelector) where TRecord:struct where TKey:IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }

      

        public static void ApplyRecordOrdering<TRecord, THeader, TKey>(this IHeaderedVariablePageBuilder<TRecord,THeader> builder, Func<TRecord, TKey> keySelector) where TRecord : struct where TKey : IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }

        public static void ApplyRecordOrdering<TRecord, TKey>(this IVariablePageBuilder<TRecord> builder, Func<TRecord, TKey> keySelector) where TRecord : struct where TKey : IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }

        public static void ApplyRecordOrdering<TRecord, TKey>(this IFixedPageBuilder<TRecord> builder, Func<TRecord, TKey> keySelector) where TRecord : struct where TKey : IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }

        public static void AsVirtualHeapPage<TRecord>(this IFixedPageBuilder<TRecord> builder, byte pageTypeToUseForMap)
          
            where TRecord:struct
        {
            var t = builder as PageDefinitionBuilder;
            CreateVirtualHeapConfig<TRecord>(t, pageTypeToUseForMap);
        }

        public static void AsVirtualHeapPage<TRecord>(this IVariablePageBuilder<TRecord> builder, byte pageTypeToUseForMap)
           
            where TRecord : struct
        {
            var t = builder as PageDefinitionBuilder;
            CreateVirtualHeapConfig<TRecord>(t, pageTypeToUseForMap);
        }

       
        private static void CreateVirtualHeapConfig<TRecord>(PageDefinitionBuilder t,byte pageTypeToUseForMap)
            where TRecord : struct
        {
            if (!(t.Config is LogicalPageManagerConfiguration))
                throw new ArgumentException("Should build upon LogicalPageManagerConfiguration");
            var e = (LogicalPageManagerConfiguration) t.Config;
            if (e.Configuration.ContainsKey(t.PageNum))
                throw new InvalidOperationException($"Page {t.PageNum} has logic defined already");
            if (e.Configuration.ContainsKey(pageTypeToUseForMap))
                throw new InvalidOperationException($"Page {pageTypeToUseForMap} was already defined. Use another free page type.");
            e.Configuration.Add(t.PageNum, new ContiniousHeapPageConfiguration<TRecord>{PageTypeNum = t.PageNum,HeaderPageType = pageTypeToUseForMap});
            e.PageMap.Add(pageTypeToUseForMap,null);
            new PageDefinitionBuilder(e, pageTypeToUseForMap).AsPageWithRecordType<HeapHeader>()
                .UsingRecordDefinition(new HeapHeader())
                .WithHeader(new LinkedPageHeaderBase());

        }
    }
}