using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Transactions;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Classes.Transactions
{
    internal sealed class TransactionContentResource<TRecord> : ISinglePhaseNotification where TRecord:struct
    {

        private ConcurrentQueue<TRecord> _changeQueue = new ConcurrentQueue<TRecord>();
        private readonly IPage<TRecord> _inner;
        private Action _removeCallback;
        private readonly IsolationLevel _isolation;
        private readonly IPhysicalLocks _locks;
        public TransactionContentResource(Action removeCallback, IPage<TRecord> inner, IsolationLevel isolation)
        {
            _inner = inner;
            _removeCallback = removeCallback;
            _isolation = isolation;
            _locks = inner as IPhysicalLocks;
        }

        private bool LockCapable => _locks != null;

        public TypedRecord<TRecord> AddRecord(TRecord type)
        {
            switch (_isolation)
            {
           //     case IsolationLevel.ReadCommitted: return _headerSetInTransaction ? _uncommitedHeader : _inner.GetHeader();
                default: throw new InvalidOperationException("No other levels except `read commited` are usable for now");
            }
            throw new NotImplementedException();
        }

        public void FreeRecord(TypedRecord<TRecord> record)
        {
            throw new NotImplementedException();
        }

        public TypedRecord<TRecord> GetRecord(PageRecordReference reference)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TypedRecord<TRecord>> IterateRecords()
        {
            throw new NotImplementedException();
        }

        public void StoreRecord(TypedRecord<TRecord> record)
        {
            throw new NotImplementedException();
        }

        public void SwapRecords(PageRecordReference record1, PageRecordReference record2)
        {

        }

        public void Commit(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        public void InDoubt(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            throw new NotImplementedException();
        }

        public void Rollback(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            throw new NotImplementedException();
        }
    }
}
