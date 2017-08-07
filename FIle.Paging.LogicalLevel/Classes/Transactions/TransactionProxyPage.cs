using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Transactions;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Classes.Transactions
{
    internal sealed class TransactionProxyPage<TRecord> : IPage<TRecord> where TRecord : TypedRecord, new()
    {
        private readonly IPage<TRecord> _inner;
        private readonly ConcurrentDictionary<Transaction, TransactionContentResource<TRecord>> _transactionBlocks = new ConcurrentDictionary<Transaction, TransactionContentResource<TRecord>>();

        public TransactionProxyPage(IPage<TRecord> inner)
        {
            _inner = inner;
        }

        public double PageFullness => throw new NotImplementedException();

        public PageReference Reference => _inner.Reference;

        public byte RegisteredPageType => _inner.RegisteredPageType;

        public bool AddRecord(TRecord type)
        {
            var store = GetStore();
            if (store != null)
                return store.AddRecord(type);
            else
                return _inner.AddRecord(type);
        }

       
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void FreeRecord(TRecord record)
        {
            var store = GetStore();
            if (store != null)
                store.FreeRecord(record);
            else
                _inner.AddRecord(record);
        }

        public TRecord GetRecord(PageRecordReference reference)
        {
            var store = GetStore();
            if (store != null)
                return store.GetRecord(reference);
            else
                return _inner.GetRecord(reference);
        }

        public IEnumerable<TRecord> IterateRecords()
        {
            var store = GetStore();
            if (store != null)
                return store.IterateRecords();
            else
                return _inner.IterateRecords();
        }

        public void StoreRecord(TRecord record)
        {
            var store = GetStore();
            if (store != null)
                 store.StoreRecord(record);
            else
                 _inner.StoreRecord(record);
        }


        private TransactionContentResource<TRecord> GetStore()
        {
            if (Transaction.Current != null)
            {
                var store = _transactionBlocks.GetOrAdd(Transaction.Current, (k) =>
                {                    
                    if (Transaction.Current.IsolationLevel == IsolationLevel.ReadUncommitted)
                        throw new InvalidOperationException("Isolation level `read uncommited` is unsupported");
                    var block = new TransactionContentResource<TRecord>(() => _transactionBlocks.TryRemove(k, out var b), _inner, Transaction.Current.IsolationLevel);
                    Transaction.Current.EnlistVolatile(block, EnlistmentOptions.None);
                    return block;
                });
                return store;
            }
            else
                return null;
        }
    }
}
