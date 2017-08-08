using File.Paging.PhysicalLevel.Classes.Pages;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    public sealed class NoTransactionConfiguration : TransactionParticipancyConfiguration
    {
        public override IPage CreateTransactionLayerPage(IPage physicalPage)
        {
            return physicalPage;
        }
    }
}