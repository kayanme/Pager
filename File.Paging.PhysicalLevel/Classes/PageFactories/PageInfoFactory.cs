using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
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
                //case VariableRecordTypePageConfiguration<TRecordType> conf:
                //    return new ComplexRecordTypePage<TRecordType>(page.Headers, page.ContentAccessor,
                //        pageNum, page.Config.PageSize, page.PageType, conf);
            }                        
            
            return new BinarySearchContext<TRecordType>( page.Headers, ser, pageNum,keyType, actionToClean);
        }
    }
}
