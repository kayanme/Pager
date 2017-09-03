using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Contracts;

namespace File.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export]
    internal sealed class LogicalLevelManipulationFactory
    {       

        public ILogicalRecordOrderManipulation CreatePage(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return page.Config.ConsistencyConfiguration.ConsistencyAbilities.HasFlag(ConsistencyAbilities.PhysicalLocks)
                ? new LogicalRecordManipulator(page.Headers, pageNum, actionToClean)
                : null;
        }
    }
}
