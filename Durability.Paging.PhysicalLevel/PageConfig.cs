using System.IO.Paging.PhysicalLevel.Configuration;

namespace Durability.Paging.PhysicalLevel
{
    public class PageConfig : PageManagerConfiguration
    {
        public PageConfig(PageSize sizeOfPage) : base(sizeOfPage)
        {
            DefinePageType(1)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new Proc1());
            DefinePageType(2)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new Proc3());
        }
    }
}