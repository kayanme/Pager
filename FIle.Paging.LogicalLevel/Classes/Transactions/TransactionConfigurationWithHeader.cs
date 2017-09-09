using System.Diagnostics;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Classes.Transactions;

namespace FIle.Paging.LogicalLevel.Classes.Configurations
{
    public sealed class TransactionConfigurationWithHeader<TRecord,THeader> : TransactionParticipancyConfiguration where TRecord : struct where THeader:new()
    {
        public int ReadLockNumber;
        public int WriteLockNumber;

        //public override IPage CreateTransactionLayerPage(IPage physicalPage)
        //{
        //    var pp = physicalPage as IHeaderedPage<THeader>;
        //    Debug.Assert(pp !=null, "pp!=null");
        //    var tp = new TransactionProxyPage<TRecord>(pp as IPage<TRecord>);
        //    var thp = new TransactionProxyHeaderedPage<TRecord,THeader>(pp,tp);
        //    return thp;
        //}
        
    }
}