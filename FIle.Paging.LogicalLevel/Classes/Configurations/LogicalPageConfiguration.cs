using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    internal abstract class LogicalPageConfiguration
    {
        public TransactionParticipancyConfiguration TransactionBehaviour;
        public abstract IPage CreateLogicalPage(IPage physicalPage);

        public IPage CreateTransactionPage(IPage physicalPage)
        {
            var proxy = TransactionBehaviour?.CreateTransactionLayerPage(physicalPage) ?? physicalPage;
            return proxy;
        }

        public bool IstransactionParticipant;

        

    }
}
