//using System;
//using System.Linq;
//using System.Transactions;
//using System.IO.Paging.PhysicalLevel.Classes;
//using System.IO.Paging.PhysicalLevel.Classes.Pages;
//using System.IO.Paging.LogicalLevel.Classes.Transactions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//using TechTalk.SpecFlow;
//using TechTalk.SpecFlow.Assist;
//using FakeItEasy;
//using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;

//namespace Test.Paging.LogicalLevel.Transactions
//{
//    [Binding]
//    public class CommonTransactionSupportSteps
//    {
//        [Given(@"configured physical page with:")]
//        public void GivenConfiguredPhysicalPageWith(Table table)
//        {
//            var withLocks = bool.Parse(table.Rows[0]["Locks"]);
//            var withVersions = bool.Parse(table.Rows[0]["Version"]);
          
//            var hp = withLocks && withVersions ? A.Fake<IPage<TestRecord>>(c=>c.Implements<IPhysicalLocks>().Implements<IRowVersionControl>())
//                : withLocks ?A.Fake<IPage<TestRecord>>(c=>c.Implements<IPhysicalLocks>())
//                    : withVersions ? A.Fake<IPage<TestRecord>>(c => c.Implements <IRowVersionControl>())
//                        : A.Fake<IPage<TestRecord>>();
          
           
//            ScenarioContext.Current.Add("pp",hp);
//        }

//        [Given(@"configured physical headered page with:")]
//        public void GivenConfiguredPhysicalHeaderedPageWith(Table table)
//        {
//            var withLocks = bool.Parse(table.Rows[0]["Locks"]);
//            var withVersions = bool.Parse(table.Rows[0]["Version"]);
         
//            var hp = withLocks && withVersions ? A.Fake<IHeaderedPage<TestRecord>>(c => c.Implements<IPhysicalLocks>().Implements<IRowVersionControl>())
//                : withLocks ? A.Fake<IHeaderedPage<TestRecord>>(c => c.Implements<IPhysicalLocks>())
//                    : withVersions ? A.Fake<IHeaderedPage<TestRecord>>(c => c.Implements<IRowVersionControl>())
//                        : A.Fake<IHeaderedPage<TestRecord>>();


//            ScenarioContext.Current.Add("pp", hp);
//            ScenarioContext.Current.Add("mr", mp);
//        }

//        [Then(@"the supported transaction levels are:")]
//        public void ThenTheSupportedTransactionLevelsAre(Table table)
//        {
            
//            var supportedLevels= table.Rows.Select(k=>Enum.Parse(typeof(IsolationLevel), k[0])).Cast<IsolationLevel>().ToArray();
//            var pp = ScenarioContext.Current["pp"] ;
           
//            foreach (IsolationLevel level in Enum.GetValues(typeof(IsolationLevel)))
//            {
//                switch (pp)
//                {
//                    case IPage<TestRecord> t:

//                        A.CallTo(() => t.AddRecord(A<TestRecord>.Ignored)).Returns(new TypedRecord<TestRecord>());
                        
//                        var rs = new TransactionContentResource<TestRecord>(() => { },t, level);
//                        try
//                        {
//                            rs.AddRecord(new TestRecord());
                            
//                            if (!supportedLevels.Contains(level))
//                                Assert.Fail($"isolation level {level} must be unsupported");
//                        }
//                        catch (InvalidOperationException e)
//                        {
//                            if (supportedLevels.Contains(level))
//                                Assert.Fail($"isolation level {level} must be supported");
//                        }
//                        break;
//                    case IHeaderedPage<TestRecord> t:
//                        A.CallTo(() => t.GetHeader()).Returns(default ( TestRecord));
                        
//                        var r2 = new TransactionHeaderResource<TestRecord>(() => { }, t, level,0,0);
//                        try
//                        {
//                            r2.GetHeader();
//                            if (!supportedLevels.Contains(level))
//                                Assert.Fail($"isolation level {level} must be unsupported");
//                        }
//                        catch (InvalidOperationException e)
//                        {
//                            if (supportedLevels.Contains(level))
//                                Assert.Fail($"isolation level {level} must be supported");
//                        }
//                        break;
//                }
//            }
                      
//        }
//    }
//}
