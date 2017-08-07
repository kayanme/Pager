using System;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    public static class ExtensionBuilder
    {
        public static void ApplyRecordOrdering<TRecord,TKey>(this IHeaderedFixedPageBuilder<TRecord> builder,Func<TRecord,TKey> keySelector) where TRecord:TypedRecord,new() where TKey:IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }

        private static void AddOrderingCommon<TRecord, TKey>(Func<TRecord, TKey> keySelector, PageDefinitionBuilder t)
            where TRecord : TypedRecord, new() where TKey : IComparable<TKey>
        {
            if (!(t.Config is LogicalPageManagerConfiguration))
                throw new ArgumentException("Should build upon LogicalPageManagerConfiguration");
            var e = t.Config as LogicalPageManagerConfiguration;
            if (e.Configuration.ContainsKey(t.PageNum))
                throw new InvalidOperationException($"Page {t.PageNum} has logic defined already");
            e.Configuration.Add(t.PageNum, new OrderedLogicalPageConfiguration<TRecord, TKey>() {KeySelector = keySelector});
        }

        public static void ApplyRecordOrdering<TRecord, TKey>(this IHeaderedVariablePageBuilder<TRecord> builder, Func<TRecord, TKey> keySelector) where TRecord : TypedRecord, new() where TKey : IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }

        public static void ApplyRecordOrdering<TRecord, TKey>(this IVariablePageBuilder<TRecord> builder, Func<TRecord, TKey> keySelector) where TRecord : TypedRecord, new() where TKey : IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }

        public static void ApplyRecordOrdering<TRecord, TKey>(this IFixedPageBuilder<TRecord> builder, Func<TRecord, TKey> keySelector) where TRecord : TypedRecord, new() where TKey : IComparable<TKey>
        {
            var t = builder as PageDefinitionBuilder;
            AddOrderingCommon(keySelector, t);
        }
    }
}