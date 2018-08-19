namespace System.IO.Paging.LogicalLevel.Classes.Transactions
{
    public sealed class TransactionConfigurationWithoutHeader<TRecord> : TransactionParticipancyConfiguration where TRecord : struct 
    {

        //public override IPage CreateTransactionLayerPage(IPage physicalPage)
        //{
        //    var pp = physicalPage as IPage<TRecord>;
        //    Debug.Assert(pp != null, "pp!=null");
        //    var tp = new TransactionProxyPage<TRecord>(pp);            
        //    return tp;
        //}

    }
}