using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations;

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
        }
    }
}
