using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Classes.Transactions;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    public sealed class TransactionConfigurationWithoutHeader<TRecord> : TransactionParticipancyConfiguration where TRecord : TypedRecord, new() 
    {

        public override IPage CreateTransactionLayerPage(IPage physicalPage)
        {
            var pp = physicalPage as IPage<TRecord>;
            Debug.Assert(pp != null, "pp!=null");
            var tp = new TransactionProxyPage<TRecord>(pp);            
            return tp;
        }

    }
}