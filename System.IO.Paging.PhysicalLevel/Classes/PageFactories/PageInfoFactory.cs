using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Implementations;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export]
    internal sealed class PageInfoFactory
    {
        public IPageInfo CreatePage(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return new PageInfo(pageNum,page.Headers,page.PageType,(ushort)page.ContentAccessor.PageSize,page.Accessor.ExtentNumber, actionToClean);
        }
    }

    [Export]
    internal sealed class BinarySearcherFactory
    {
        public IBinarySearcher<TRecordType> CreatePage<TRecordType>(BufferedPage page, PageReference pageNum, Action actionToClean) where TRecordType : struct
        {
            RecordAcquirer<TRecordType> ser = null;
            KeyPersistanseType keyType = KeyPersistanseType.Physical;
            switch (page.Config)
            {
                case FixedRecordTypePageConfiguration<TRecordType> conf:
                    ser = new RecordAcquirer<TRecordType>(page.ContentAccessor, conf.RecordMap);
                    keyType = conf.WithLogicalSort ? KeyPersistanseType.Logical : KeyPersistanseType.Physical;
                    break;
                case VariableRecordTypePageConfiguration<TRecordType> conf:
                    ser = new RecordAcquirer<TRecordType>(page.ContentAccessor, conf.RecordMap);
                    keyType = conf.WithLogicalSort ? KeyPersistanseType.Logical : KeyPersistanseType.Physical;
                    break;
            }
            return new BinarySearchContext<TRecordType>(page.Headers, ser, pageNum, keyType, actionToClean);
        }
    }
}
