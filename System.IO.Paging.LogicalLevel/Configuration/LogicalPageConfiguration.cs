using System.IO.Paging.LogicalLevel.Classes.Transactions;

namespace System.IO.Paging.LogicalLevel.Configuration
{
    internal abstract class LogicalPageConfiguration
    {
       
        public TransactionParticipancyConfiguration TransactionBehaviour;
       
        //public IPage CreateTransactionPage(IPage physicalPage)
        //{
        //    var proxy = TransactionBehaviour?.CreateTransactionLayerPage(physicalPage) ?? physicalPage;
        //    return proxy;
        //}

        public bool IstransactionParticipant;

        

    }
}
