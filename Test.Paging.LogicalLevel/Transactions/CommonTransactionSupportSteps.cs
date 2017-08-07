using System;
using System.Linq;
using System.Transactions;
using File.Paging.PhysicalLevel.Classes.Pages;
using FIle.Paging.LogicalLevel.Classes.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Test.Paging.LogicalLevel.Transactions
{
    [Binding]
    public class CommonTransactionSupportSteps
    {
        [Given(@"configured physical page with:")]
        public void GivenConfiguredPhysicalPageWith(Table table)
        {
            var withLocks = bool.Parse(table.Rows[0]["Locks"]);
            var withVersions = bool.Parse(table.Rows[0]["Version"]);
            var mp = new MockRepository();
            var hp = withLocks && withVersions ? mp.StrictMultiMock<IPage<TestRecord>>(typeof(IPhysicalLocks), typeof(IRowVersionControl))
                : withLocks ? mp.StrictMultiMock<IPage<TestRecord>>(typeof(IPhysicalLocks))
                    : withVersions ? mp.StrictMultiMock<IPage<TestRecord>>(typeof(IRowVersionControl))
                        : mp.StrictMock<IPage<TestRecord>>();
          
           
            ScenarioContext.Current.Add("pp",hp);
        }

        [Given(@"configured physical headered page with:")]
        public void GivenConfiguredPhysicalHeaderedPageWith(Table table)
        {
            var withLocks = bool.Parse(table.Rows[0]["Locks"]);
            var withVersions = bool.Parse(table.Rows[0]["Version"]);
            var mp = new MockRepository();
            var hp = withLocks && withVersions ? mp.StrictMultiMock<IHeaderedPage<TestRecord>>(typeof(IPhysicalLocks), typeof(IRowVersionControl))
                : withLocks ? mp.StrictMultiMock<IHeaderedPage<TestRecord>>(typeof(IPhysicalLocks))
                    : withVersions ? mp.StrictMultiMock<IHeaderedPage<TestRecord>>(typeof(IRowVersionControl))
                        : mp.StrictMock<IHeaderedPage<TestRecord>>();


            ScenarioContext.Current.Add("pp", hp);
            ScenarioContext.Current.Add("mr", mp);
        }

        [Then(@"the supported transaction levels are:")]
        public void ThenTheSupportedTransactionLevelsAre(Table table)
        {
            
            var supportedLevels= table.Rows.Select(k=>Enum.Parse(typeof(IsolationLevel), k[0])).Cast<IsolationLevel>().ToArray();
            var pp = ScenarioContext.Current["pp"] ;
           
            foreach (IsolationLevel level in Enum.GetValues(typeof(IsolationLevel)))
            {
                switch (pp)
                {
                    case IPage<TestRecord> t:
                        t.BackToRecord();
                        t.Expect(k => k.AddRecord(null)).IgnoreArguments().Return(true).Repeat.Any();
                        t.Replay();
                        var rs = new TransactionContentResource<TestRecord>(() => { },t, level);
                        try
                        {
                            rs.AddRecord(new TestRecord());
                            if (!supportedLevels.Contains(level))
                                Assert.Fail($"isolation level {level} must be unsupported");
                        }
                        catch (InvalidOperationException e)
                        {
                            if (supportedLevels.Contains(level))
                                Assert.Fail($"isolation level {level} must be supported");
                        }
                        break;
                    case IHeaderedPage<TestRecord> t:
                        t.BackToRecord();
                        t.Expect(k => k.GetHeader()).IgnoreArguments().Return(null).Repeat.Any();
                        t.Replay();
                        var r2 = new TransactionHeaderResource<TestRecord>(() => { }, t, level,0,0);
                        try
                        {
                            r2.GetHeader();
                            if (!supportedLevels.Contains(level))
                                Assert.Fail($"isolation level {level} must be unsupported");
                        }
                        catch (InvalidOperationException e)
                        {
                            if (supportedLevels.Contains(level))
                                Assert.Fail($"isolation level {level} must be supported");
                        }
                        break;
                }
            }
                      
        }
    }
}
