using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Implementations;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
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
