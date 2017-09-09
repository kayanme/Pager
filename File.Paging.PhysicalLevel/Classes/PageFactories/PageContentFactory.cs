using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Implementations;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
{


    [Export]
    internal sealed class PageContentFactory
    {


        public IPage<TRecordType> CreatePage<TRecordType>(BufferedPage page, PageReference pageNum, Action actionToClean)
            where TRecordType : struct
        {
            switch (page.Config)
            {
                case FixedRecordTypePageConfiguration<TRecordType> conf:
                    var ser = new RecordAcquirer<TRecordType>(page.ContentAccessor,conf.RecordMap);
                    return new FixedRecordTypedPage<TRecordType>(
                        page.Headers, ser, pageNum, conf,actionToClean);
                //case VariableRecordTypePageConfiguration<TRecordType> conf:
                //    return new ComplexRecordTypePage<TRecordType>(page.Headers, page.ContentAccessor,
                //        pageNum, page.Config.PageSize, page.PageType, conf);
            }
            throw new Exception();
        }
    }
}
