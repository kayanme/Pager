using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.Threading;
using System.Transactions;

namespace System.IO.Paging.LogicalLevel.Classes.Transactions
{
    internal sealed class TransactionHeaderResource<THeader> :  ISinglePhaseNotification where THeader:new()
    {
        private THeader _replacedHeader;
        private THeader _uncommitedHeader;
        private bool _headerSetInTransaction;
        private readonly IHeaderedPage<THeader> _inner;
        private readonly Action _removeCallback;
        private readonly IsolationLevel _isolation;
        private readonly byte _readlock;
        private readonly byte _writelock;
        private IPhysicalLocks _locks;
        public TransactionHeaderResource(Action removeCallback,IHeaderedPage<THeader> inner,IsolationLevel isolation, byte readlock, byte writelock )
        {
            _inner = inner;
            _removeCallback = removeCallback;
            _isolation = isolation;
            _readlock = readlock;
            _writelock = writelock;
            _locks = _inner as IPhysicalLocks;
            
        }
        
        public THeader GetHeader()
        {
            switch (_isolation)
            {
                case IsolationLevel.ReadCommitted:
                    if (_headerSetInTransaction)
                        return _uncommitedHeader;
                    if (_locks != null)
                    {
                        var pageLock = default(LockToken<PageReference>);
                        try
                        {
                            var t = _locks.WaitPageLock(_readlock);
                            t.RunSynchronously();
                            pageLock = t.Result;
                            return _inner.GetHeader();
                        }
                       finally
                        {
                            if (pageLock.Equals(default(LockToken<PageReference>)))
                               _locks.ReleasePageLock(pageLock);
                        }
                                           
                    }
                    else
                    {
                        return  _inner.GetHeader();

                    }
                default:throw new InvalidOperationException("No other levels except `read commited` are usable for now");
            }
        }

        private LockToken<PageReference>? _pageLock;

        ~TransactionHeaderResource()
        {
            if (_pageLock !=null)    
                _locks.ReleasePageLock(_pageLock.Value);
        }

        public void SetHeader(THeader header)
        {
            switch (_isolation)
            {
                case IsolationLevel.ReadCommitted:
                    if (_locks != null)
                    {
                        var t = _locks.WaitPageLock(_writelock);
                        t.RunSynchronously();
                        _pageLock = t.Result;
                    }
                    _uncommitedHeader = header;
                    _headerSetInTransaction = true;
                    break;                       
                    
                 
                default: throw new InvalidOperationException("No other levels except `read commited` are usable for now");
            }
           
          
        }

        public void Commit(Enlistment enlistment)
        {
            if (_pageLock != null)
            {
                Thread.BeginCriticalRegion();
                _locks.ReleasePageLock(_pageLock.Value);
                _locks = null;
                Thread.EndCriticalRegion();
            }
            enlistment?.Done();
            _removeCallback();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {
                if (_headerSetInTransaction)
                {
                    _replacedHeader = _inner.GetHeader();
                    _inner.ModifyHeader(_uncommitedHeader);
                }
                preparingEnlistment?.Prepared();
            }
            catch
            {
                preparingEnlistment?.ForceRollback();
                throw;
            }
            finally
            {
                preparingEnlistment?.Done();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            try
            {
                _inner.ModifyHeader(_replacedHeader);
            }

            finally
            {
                if (_pageLock != null)
                {
                    Thread.BeginCriticalRegion();
                    _locks.ReleasePageLock(_pageLock.Value);
                    _locks = null;
                    Thread.EndCriticalRegion();
                }
            }
            enlistment?.Done();
            _removeCallback();
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            try
            {
                if (_headerSetInTransaction)
                {
                    _inner.ModifyHeader(_uncommitedHeader);                    
                }
                singlePhaseEnlistment?.Committed();                
            }
            catch
            {
                singlePhaseEnlistment?.Aborted();
            }
            finally
            {
                if (_pageLock != null)
                {
                    Thread.BeginCriticalRegion();
                    _locks.ReleasePageLock(_pageLock.Value);
                    _locks = null;
                    Thread.EndCriticalRegion();
                }
            }
            singlePhaseEnlistment?.Done();
            _removeCallback();
        }
    }
}
