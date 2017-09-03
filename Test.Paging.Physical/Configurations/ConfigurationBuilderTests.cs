﻿using System;
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
            Action<TestRecord,byte[]> filler = (a, b) => { b[0] = 1; };
            Action<byte[], TestRecord> getter = (a, b) => { a[0] = 2; };
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(filler,getter,10));

            tc.Verify();

            VerifyFixedRecordCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariable()
        {
            Action<TestRecord, byte[]> filler = (a, b) => { b[0] = 1; };
            Action<byte[], TestRecord> getter = (a, b) => { a[0] = 2; };
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DefineOnlyNumberTypeAndMultipleTypes()
        {            
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().WithMultipleTypeRecord(k=>1));

            tc.Verify();

            
        }

        private static IVariableSizeRecordDefinition<TestRecord> VariableRecordDefinitionCommon()
        {
            Action<TestRecord, byte[]> filler = (a, b) => { b[0] = 1; };
            Action<byte[], TestRecord> getter = (a, b) => { a[0] = 2; };
            var ti = MockRepository.GenerateStub<IVariableSizeRecordDefinition<TestRecord>>();
            ti.Expect(k => k.FillBytes(null, null)).IgnoreArguments().Do(filler);
            ti.Expect(k => k.FillFromBytes(null, null)).IgnoreArguments().Do(getter);
            ti.Expect(k => k.Size(null)).IgnoreArguments().Return(10);
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
            Assert.AreEqual(1, c2.RecordMap.Count);
            Assert.IsTrue(c2.RecordMap.ContainsKey(1));
            var c3 = c2.RecordMap[1];

            Assert.AreEqual(10, c3.GetSize(null));
            var t = new byte[1];
            c3.FillBytes(null, t);
            Assert.AreEqual(1, t[0]);
            c3.FillFromBytes(t, null);
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
            c2.RecordMap.FillBytes(null, t);
            Assert.AreEqual(1, t[0]);
            c2.RecordMap.FillFromBytes(t, null);
            Assert.AreEqual(2, t[0]);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAlt()
        {
            Action<TestRecord, byte[]> filler = (a, b) => { b[0] = 1; };
            Action<byte[], TestRecord> getter = (a, b) => { a[0] = 2; };
            var definition = MockRepository.GenerateStub<IFixedSizeRecordDefinition<TestRecord>>();
            definition.Expect(k => k.FillBytes(null, null)).IgnoreArguments().Do(filler);
            definition.Expect(k => k.FillFromBytes(null, null)).IgnoreArguments().Do(getter);
            definition.Expect(k => k.Size).Return(10);
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(definition));

            tc.Verify();

            VerifyFixedRecordCommon(tc);
        }


        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAndConsistency()
        {
            Action<TestRecord, byte[]> filler = (a, b) => { b[0] = 1; };
            Action<byte[], TestRecord> getter = (a, b) => { a[0] = 2; };
            var definition = MockRepository.GenerateStub<IFixedSizeRecordDefinition<TestRecord>>();
            definition.Expect(k => k.FillBytes(null, null)).IgnoreArguments().Do(filler);
            definition.Expect(k => k.FillFromBytes(null, null)).IgnoreArguments().Do(getter);
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
            c2.Header.FillBytes(null, t2);
            Assert.AreEqual(3, t2[0]);
            c2.Header.FillFromBytes(t2, null);
            Assert.AreEqual(4, t2[0]);
        }

        private static IHeaderDefinition<TestHeader> CommonHeaderDefinition()
        {
            Action<TestHeader, byte[]> filler = (a, b) => { b[0] = 3; };
            Action<byte[], TestHeader> getter = (a, b) => { a[0] = 4; };
            var t = MockRepository.GenerateStub<IHeaderDefinition<TestHeader>>();
            t.Expect(k => k.FillBytes(null, null)).IgnoreArguments().Do(filler);
            t.Expect(k => k.FillFromBytes(null, null)).IgnoreArguments().Do(getter);
            t.Expect(k => k.Size).Return(10);
            return t;
        }

        private static IFixedSizeRecordDefinition<TestRecord> CommonFixedRecordDefinition()
        {
            Action<TestRecord, byte[]> filler = (a, b) => { b[0] = 1; };
            Action<byte[], TestRecord> getter = (a, b) => { a[0] = 2; };
            var definition = MockRepository.GenerateStub<IFixedSizeRecordDefinition<TestRecord>>();
            definition.Expect(k => k.FillBytes(null, null)).IgnoreArguments().Do(filler);
            definition.Expect(k => k.FillFromBytes(null, null)).IgnoreArguments().Do(getter);
            definition.Expect(k => k.Size).Return(10);
            return definition;
        }
    }
}