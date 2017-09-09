using System;
using File.Paging.PhysicalLevel.Classes.Configurations;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Pager.Configurations
{
    public class TestingConfig:PageManagerConfiguration
    {
        public TestingConfig(PageSize size,params Action<Func<byte,IPageDefinitionBuilder>>[] builders):base(size)
        {
            foreach (var builder in builders)
            {
                builder(DefinePageType);
            }
        }

        
    }


    [TestClass]
    public class ConfigurationBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DefineOnlyNumber()
        {
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp=>dp(1));
            Assert.AreEqual(PageManagerConfiguration.PageSize.Kb4,tc.SizeOfPage);
            tc.Verify();

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DefineOnlyNumberAndType()
        {
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>());

            tc.Verify();

        }

        [TestMethod]        
        public void DefineOnlyNumberTypeAndFixedRecord()
         {
           Getter<TestRecord> filler = (ref TestRecord a,byte[] b) => { b[0] = 1; };
             Setter<TestRecord> getter = (byte[] a,ref TestRecord b) => { a[0] = 2; };
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(filler,getter,10));

            tc.Verify();

            VerifyFixedRecordCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariable()
        {
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(filler, getter, (r)=>10));

            tc.Verify();

            VerifyVariableCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariableAlt()
        {
           
            var ti = VariableRecordDefinitionCommon();

            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(ti));

            tc.Verify();

            VerifyVariableCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariableWithConsistency()
        {

            var ti = VariableRecordDefinitionCommon();
            var locks = MockRepository.GenerateStub<LockRuleset>();
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(ti)
                .ApplyLockScheme(locks));

            tc.Verify();

            VerifyVariableCommon(tc, locks, false);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariableWithHeaders()
        {

            var ti = VariableRecordDefinitionCommon();
            var header = CommonHeaderDefinition();
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(ti)
                .WithHeader(header));

            tc.Verify();

            VerifyVariableCommon(tc);
            VerifyHeaderCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariableWithHeadersAndConsistency()
        {

            var ti = VariableRecordDefinitionCommon();
            var header = CommonHeaderDefinition();
            var locks = MockRepository.GenerateStub<LockRuleset>();
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(ti)
                    .WithHeader(header)
                    .ApplyLockScheme(locks));

            tc.Verify();

            VerifyVariableCommon(tc, locks, false);
            VerifyHeaderCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariableWithHeadersAndConsistencyAndSlotInfo()
        {

            var ti = VariableRecordDefinitionCommon();
            var header = CommonHeaderDefinition();
            var locks = MockRepository.GenerateStub<LockRuleset>();
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(ti)
                    .WithHeader(header)
                    .ApplyLockScheme(locks)
                    .ApplyLogicalSortIndex());

            tc.Verify();

            VerifyVariableCommon(tc, locks, true);
            VerifyHeaderCommon(tc);
        }

      

        private static IVariableSizeRecordDefinition<TestRecord> VariableRecordDefinitionCommon()
        {
            void Filler(ref TestRecord a, byte[] b)
            {
                b[0] = 1;
            }

            void Getter(byte[] a,ref TestRecord b)
            {
                a[0] = 2;
            }

            var t = default(TestRecord);
            var ti = MockRepository.GenerateStub<IVariableSizeRecordDefinition<TestRecord>>();
            ti.Expect(k => k.FillBytes(ref t, null)).IgnoreArguments().Do((Getter<TestRecord>)Filler);
            ti.Expect(k => k.FillFromBytes(null,ref t)).IgnoreArguments().Do((Setter<TestRecord>)Getter);
            ti.Expect(k => k.Size(default(TestRecord))).IgnoreArguments().Return(10);
            return ti;
        }

        private static void VerifyVariableCommon(TestingConfig tc,LockRuleset locks = null,bool slotInfo = false)
        {
            Assert.AreEqual(1, tc.PageMap.Count);
            Assert.IsTrue(tc.PageMap.ContainsKey(1));
            var c = tc.PageMap[1];
            Assert.AreEqual(typeof(TestRecord), c.RecordType);
            if (locks == null)
            {
                Assert.AreEqual(ConsistencyAbilities.None, c.ConsistencyConfiguration.ConsistencyAbilities);
            }
            else
            {
                Assert.AreEqual(ConsistencyAbilities.PhysicalLocks, c.ConsistencyConfiguration.ConsistencyAbilities);
                Assert.AreEqual(locks,c.ConsistencyConfiguration.LockRules);
            }
            Assert.IsInstanceOfType(c, typeof(VariableRecordTypePageConfiguration<TestRecord>));
            var c2 = c as VariableRecordTypePageConfiguration<TestRecord>;
            Assert.IsNotNull(c2.RecordMap);
            Assert.AreEqual(slotInfo, c2.WithLogicalSort);
         
            var c3 = c2.RecordMap;

            Assert.AreEqual(10, c3.GetSize(default(TestRecord)));
            var t = new byte[1];
            var t2 = default(TestRecord);
            c3.FillBytes(ref t2, t);
            Assert.AreEqual(1, t[0]);
            c3.FillFromBytes(t,ref t2);
            Assert.AreEqual(2, t[0]);
        }

        private static void VerifyFixedRecordCommon(TestingConfig tc,LockRuleset lockRules = null)
        {
            Assert.AreEqual(1, tc.PageMap.Count);
            Assert.IsTrue(tc.PageMap.ContainsKey(1));
            var c = tc.PageMap[1];          
            Assert.AreEqual(typeof(TestRecord), c.RecordType);
            if (lockRules ==null)
            Assert.AreEqual(ConsistencyAbilities.None, c.ConsistencyConfiguration.ConsistencyAbilities);
            else
            {
                Assert.AreEqual(ConsistencyAbilities.PhysicalLocks, c.ConsistencyConfiguration.ConsistencyAbilities);
                Assert.AreEqual(lockRules, c.ConsistencyConfiguration.LockRules);
            }
            Assert.IsInstanceOfType(c, typeof(FixedRecordTypePageConfiguration<TestRecord>));
            var c2 = c as FixedRecordTypePageConfiguration<TestRecord>;
            Assert.IsNotNull(c2.RecordMap);
            Assert.AreEqual(10, c2.RecordMap.GetSize);
            var t = new byte[1];
            var t2 = default(TestRecord);
            c2.RecordMap.FillBytes(ref t2, t);
            Assert.AreEqual(1, t[0]);
            c2.RecordMap.FillFromBytes(t,ref t2);
            Assert.AreEqual(2, t[0]);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAlt()
        {
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var definition = MockRepository.GenerateStub<IFixedSizeRecordDefinition<TestRecord>>();
            var t2 = default(TestRecord);
            definition.Expect(k => k.FillBytes(ref t2, null)).IgnoreArguments().Do(filler);
            definition.Expect(k => k.FillFromBytes(null,ref t2)).IgnoreArguments().Do(getter);
            definition.Expect(k => k.Size).Return(10);
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(definition));

            tc.Verify();

            VerifyFixedRecordCommon(tc);
        }


        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAndConsistency()
        {
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var definition = MockRepository.GenerateStub<IFixedSizeRecordDefinition<TestRecord>>();
            var t2 = default(TestRecord);
            definition.Expect(k => k.FillBytes(ref t2, null)).IgnoreArguments().Do(filler);
            definition.Expect(k => k.FillFromBytes(null, ref t2)).IgnoreArguments().Do(getter);
            definition.Expect(k => k.Size).Return(10);
            var locks = MockRepository.GenerateStub<LockRuleset>();
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>()
                           .UsingRecordDefinition(definition)
                           .ApplyLockScheme(locks));

            tc.Verify();

            VerifyFixedRecordCommon(tc, locks);                     
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAndHeaders()
        {
            var t = CommonHeaderDefinition();

            var definition = CommonFixedRecordDefinition();
                     
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition(definition)
                    .WithHeader(t));

            tc.Verify();

            VerifyFixedRecordCommon(tc);

            VerifyHeaderCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAndHeadersAndConsitency()
        {
            var t = CommonHeaderDefinition();

            var definition = CommonFixedRecordDefinition();
            var locks = MockRepository.GenerateStub<LockRuleset>();
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition(definition)
                    .ApplyLockScheme(locks)
                    .WithHeader(t));

            tc.Verify();

            VerifyFixedRecordCommon(tc, locks);

            VerifyHeaderCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAndHeadersAndConsitency2()
        {
            var t = CommonHeaderDefinition();

            var definition = CommonFixedRecordDefinition();
            var locks = MockRepository.GenerateStub<LockRuleset>();
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>()
                    .UsingRecordDefinition(definition)                   
                    .WithHeader(t)
                    .ApplyLockScheme(locks));

            tc.Verify();

            VerifyFixedRecordCommon(tc, locks);

            VerifyHeaderCommon(tc);
        }

        private static void VerifyHeaderCommon(TestingConfig tc)
        {
            Assert.AreEqual(1, tc.HeaderConfig.Count);
            var c = tc.HeaderConfig[1];
            Assert.AreEqual(tc.PageMap[1], c.InnerPageMap);
            Assert.IsInstanceOfType(c, typeof(PageHeadersConfiguration<TestHeader>));
            var c2 = c as PageHeadersConfiguration< TestHeader>;
            Assert.IsNotNull(c2.Header);
            Assert.AreEqual(10, c2.Header.GetSize);
            var t2 = new byte[1];
            var t3 = new TestHeader{Value = 3};
            c2.Header.FillBytes(ref t3, t2);
            Assert.AreEqual(3, t2[0]);
            c2.Header.FillFromBytes(t2, ref t3);
            Assert.AreEqual(4, t2[0]);
        }

        private static IHeaderDefinition<TestHeader> CommonHeaderDefinition()
        {
            Getter<TestHeader> filler = (ref TestHeader a, byte[] b) => { b[0] = 3; };
            Setter<TestHeader> getter = (byte[] a, ref TestHeader b) => { a[0] = 4; };
            var t = MockRepository.GenerateStub<IHeaderDefinition<TestHeader>>();
            var t3 = default(TestHeader);
            t.Expect(k => k.FillBytes(ref t3, null)).IgnoreArguments().Do(filler);
            t.Expect(k => k.FillFromBytes(null, ref t3)).IgnoreArguments().Do(getter);
            t.Expect(k => k.Size).Return(10);
            return t;
        }

        private static IFixedSizeRecordDefinition<TestRecord> CommonFixedRecordDefinition()
        {
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var definition = MockRepository.GenerateStub<IFixedSizeRecordDefinition<TestRecord>>();
            var t3 = default(TestRecord);
            definition.Expect(k => k.FillBytes(ref t3, null)).IgnoreArguments().Do(filler);
            definition.Expect(k => k.FillFromBytes(null, ref t3)).IgnoreArguments().Do(getter);
            definition.Expect(k => k.Size).Return(10);
            return definition;
        }
    }
}
