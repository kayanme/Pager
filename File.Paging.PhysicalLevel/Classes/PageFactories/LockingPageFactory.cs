﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    internal sealed class LockingPageFactory
    {
        private readonly IPhysicalLockManager<PageReference> _pageLockManager;
        private readonly IPhysicalLockManager<PageRecordReference> _pageRecordLockManager;

        [ImportingConstructor]
        public LockingPageFactory([Import(AllowDefault = true)]IPhysicalLockManager<PageReference> pageLockManager,
            [Import(AllowDefault = true)]IPhysicalLockManager<PageRecordReference> pageRecordLockManager)
        {
            this._pageLockManager = pageLockManager??new LockManager<PageReference>();
            this._pageRecordLockManager = pageRecordLockManager??new LockManager<PageRecordReference>();
        }        

        public IPhysicalLocks CreatePage(BufferedPage page, PageReference pageNum, Action actionToClean)
        {
            return new LockingPage(_pageLockManager, _pageRecordLockManager, 
                new LockMatrix(page.Config.ConsistencyConfiguration.LockRules), pageNum, actionToClean);           
        }
    }
}