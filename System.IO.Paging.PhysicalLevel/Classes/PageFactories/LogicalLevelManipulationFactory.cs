using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Implementations;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export]
    internal sealed class LogicalLevelManipulationFactory
    {       

        public ILogicalRecordOrderManipulation CreatePage<TRecord>(BufferedPage page, PageReference pageNum, Action actionToClean) where TRecord:struct
        {
            var config = page.Config is FixedRecordTypePageConfiguration<TRecord>
                ? (page.Config as FixedRecordTypePageConfiguration<TRecord>).RecordMap as RecordDeclaration<TRecord>
                : (page.Config as VariableRecordTypePageConfiguration<TRecord>).RecordMap as RecordDeclaration<TRecord>;
            var ser = new RecordAcquirer<TRecord>(page.ContentAccessor, config);
            return page.Config.WithLogicalSort
                ? new LogicalRecordManipulator(page.Headers, pageNum, actionToClean)
                : null;
        }
    }
}
