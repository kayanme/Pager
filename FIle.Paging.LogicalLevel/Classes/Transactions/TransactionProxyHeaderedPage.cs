using System;
using System.Collections.Concurrent;
using System.Transactions;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Classes.Transactions
{
    internal sealed class TransactionProxyHeaderedPage<TRecord,THeader> :IHeaderedPage<THeader>,IHeaderedPageInt<TRecord> 
        where THeader:new()
        where TRecord : TypedRecord, new()
    {
        private readonly byte _readlock;
        private readonly byte _writellock;
        private readonly ConcurrentDictionary<Transaction, TransactionHeaderResource<THeader>> _transactionBlocks = new ConcurrentDictionary<Transaction, TransactionHeaderResource<THeader>>();

        private readonly IHeaderedPage<THeader> _inner;

        public TransactionProxyHeaderedPage(IHeaderedPage<THeader> inner, IPage innerContent,byte readlock,byte writellock):this(inner,innerContent)
        {
            _readlock = readlock;
            _writellock = writellock;
        }

        public TransactionProxyHeaderedPage(IHeaderedPage<THeader> inner, IPage innerContent)
        {
            _inner = inner;
            Content = innerContent;
        }

        public IPage Content { get; private set; }

        public double PageFullness => Content.PageFullness;
        public int UsedRecords
        {
            get { return _inner.UsedRecords; }
        }

        public PageReference Reference => _inner.Reference;

        public byte RegisteredPageType => Content.RegisteredPageType;

        public void Dispose()
        {

            _inner.Dispose();
            Content.Dispose();
        }

     

        public THeader GetHeader()
        {
            var store = GetStore();
            if (store != null)return store.GetHeader();
            else return _inner.GetHeader(); 
        }

        private TransactionHeaderResource<THeader> GetStore()
        {
            if (Transaction.Current != null)
            {
                var store = _transactionBlocks.GetOrAdd(Transaction.Current, (k) =>
                {
                    
                    if (Transaction.Current.IsolationLevel == IsolationLevel.ReadUncommitted)
                        throw new InvalidOperationException("Isolation level `read uncommited` is unsupported");
                    var block = new TransactionHeaderResource<THeader>(() => _transactionBlocks.TryRemove(k, out var b), _inner,
                        Transaction.Current.IsolationLevel,_readlock,_writellock);
                    Transaction.Current.EnlistVolatile(block, EnlistmentOptions.None);
                    return block;
                });
                return store;
            }
            else
                return null;
        }

        public void ModifyHeader(THeader header)
        {
            var store = GetStore();
            if (store != null)  store.SetHeader(header);
            else _inner.ModifyHeader(header);
        }

        public void SwapContent(IPage<TRecord> page)
        {
            Content = page;
        }
    }
}
