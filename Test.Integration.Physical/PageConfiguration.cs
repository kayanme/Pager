using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace Test.Integration.Physical
{


    internal class PageConfiguration:PageManagerConfiguration
    {
        public PageConfiguration() : base(PageSize.Kb8)
        {
            DefinePageType(1)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new TestRecordGetter());

            DefinePageType(2)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new TestRecordGetter())
                .WithHeader(new TestHeaderGetter());

            DefinePageType(3)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new TestRecordGetter())
                .ApplyLogicalSortIndex();

            DefinePageType(4)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new TestRecordGetter())
                .ApplyLockScheme(new ReaderWriterLockRuleset());
        }
    }
}
