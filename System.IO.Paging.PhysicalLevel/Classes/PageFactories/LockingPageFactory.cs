﻿using System.ComponentModel.Composition;
using System.IO.Paging.PhysicalLevel.Classes.Pages;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Implementations;

namespace System.IO.Paging.PhysicalLevel.Classes.PageFactories
{
    [Export]
    internal sealed class LockingPageFactory
    {
        private readonly ILockManager<PageReference> _pageLockManager;
        private readonly ILockManager<PageRecordReference> _pageRecordLockManager;

        [ImportingConstructor]
        public LockingPageFactory([Import(AllowDefault = true)]ILockManager<PageReference> pageLockManager,
            [Import(AllowDefault = true)]ILockManager<PageRecordReference> pageRecordLockManager)
        {
            this._pageLockManager = pageLockManager??new LockManager<PageReference>();
            this._pageRecordLockManager = pageRecordLockManager??new LockManager<PageRecordReference>();
        }        

        public IPhysicalLocks CreatePage(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return page.Config.ConsistencyConfiguration.ConsistencyAbilities.HasFlag(ConsistencyAbilities.PhysicalLocks)
                ? new LockingPage(_pageLockManager, _pageRecordLockManager,
                    page.Config.ConsistencyConfiguration.LockRules, pageNum, actionToClean)
                : null;
          
        }
    }
}
