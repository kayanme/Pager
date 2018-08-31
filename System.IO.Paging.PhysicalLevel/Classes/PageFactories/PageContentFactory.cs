using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Implementations;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{


    [Export]
    internal sealed class PageContentFactory
    {


        public IPage<TRecordType> CreatePage<TRecordType>(BufferedPage page, PageReference pageNum, Action actionToClean)
            where TRecordType : struct
        {
            if (typeof(TRecordType) != page.Config.RecordType)
                throw new ArgumentException($"The requested page record type {typeof(TRecordType)} does not match the one in page configuration ({page.Config.RecordType})");
            switch (page.Config)
            {
                case ImageTypePageConfiguration<TRecordType> conf:
                    var ser = new RecordAcquirer<TRecordType>(page.ContentAccessor, conf.RecordMap);
                    return new FixedRecordTypedPage<TRecordType>(
                        page.Headers, ser, pageNum, conf, actionToClean);
                case FixedRecordTypePageConfiguration<TRecordType> conf:
                    ser = new RecordAcquirer<TRecordType>(page.ContentAccessor,conf.RecordMap);
                    return new FixedRecordTypedPage<TRecordType>(
                        page.Headers, ser, pageNum, conf,actionToClean);               
                case VariableRecordTypePageConfiguration<TRecordType> conf:
                    return new ComplexRecordTypePage<TRecordType>(page.Headers, page.ContentAccessor,
                        pageNum, page.Config.PageSize, page.PageType, conf,actionToClean);
            }
            throw new Exception();
        }
    }
}
