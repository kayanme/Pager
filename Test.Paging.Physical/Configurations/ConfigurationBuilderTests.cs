using System;
using System.Collections.Generic;
using FakeItEasy;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Configuration.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements;

namespace Test.Paging.PhysicalLevel.Configurations
{
    public class TestingConfig : PageManagerConfiguration
    {
        public TestingConfig(PageSize size, params Action<Func<byte, IPageDefinitionBuilder>>[] builders) : base(size)
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
                dp => dp(1));
            Assert.AreEqual(PageManagerConfiguration.PageSize.Kb4, tc.SizeOfPage);
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
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(filler, getter, 10));

            tc.Verify();

            VerifyFixedRecordCommon(tc);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndOneRecordVariable()
        {
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().UsingRecordDefinition(filler, getter, (r) => 10));

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
            var locks = A.Fake<LockRuleset>();
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
            var locks = A.Fake<LockRuleset>();
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
            var locks = A.Fake<LockRuleset>();
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
        public void DefineOnlyNumberTypeAndOneRecordImageTypeWithCorrectSize()
        {

            var ti = CommonFixedRecordDefinition(4096);                        
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().AsPlainImage(ti));

            tc.Verify();

            VerifyImageRecordCommon(tc);
            Assert.IsFalse(tc.HeaderConfig.ContainsKey(1));
            var pageHeaders = tc.PageMap[1].ReturnHeaderInfo();
            Assert.AreEqual(4096,pageHeaders.RecordSize);
            Assert.IsTrue(pageHeaders.IsFixed);
            Assert.IsFalse(pageHeaders.WithLogicalSort);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DefineOnlyNumberTypeAndOneRecordImageTypeWithIncorrectSize()
        {

            var ti = CommonFixedRecordDefinition(10);
            var tc = new TestingConfig(PageManagerConfiguration.PageSize.Kb4,
                dp => dp(1).AsPageWithRecordType<TestRecord>().AsPlainImage(ti));

            tc.Verify();
           
        }



        private static IVariableSizeRecordDefinition<TestRecord> VariableRecordDefinitionCommon()
        {
            void Filler(ref TestRecord a, byte[] b)
            {
                b[0] = 1;
            }

            void Getter(byte[] a, ref TestRecord b)
            {
                a[0] = 2;
            }

            var t = default(TestRecord);
            var ti = A.Fake<IVariableSizeRecordDefinition<TestRecord>>();
            A.CallTo(() => ti.FillBytes(ref t, null))
                .WithAnyArguments()
                .AssignsOutAndRefParametersLazily(a =>
                {
                    var arg0 = (TestRecord)a.Arguments[0];
                    Filler(ref arg0, (byte[])a.Arguments[1]);
                    return new[] { arg0 as object } as ICollection<object>;
                });

            A.CallTo(() => ti.FillFromBytes(null, ref t)).WithAnyArguments()
            .AssignsOutAndRefParametersLazily(a =>
            {
                var arg1 = (TestRecord)a.Arguments[1];
                Getter((byte[])a.Arguments[0], ref arg1);
                return new[] { arg1 as object } as ICollection<object>;
            });
            A.CallTo(() => ti.Size(default(TestRecord))).WithAnyArguments().Returns(10);
            return ti;
        }

        private static void VerifyVariableCommon(TestingConfig tc, LockRuleset locks = null, bool slotInfo = false)
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
                Assert.AreEqual(locks, c.ConsistencyConfiguration.LockRules);
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
            c3.FillFromBytes(t, ref t2);
            Assert.AreEqual(2, t[0]);
        }

        private static void VerifyFixedRecordCommon(TestingConfig tc, LockRuleset lockRules = null,int recordSize = 10)
        {
            Assert.AreEqual(1, tc.PageMap.Count);
            Assert.IsTrue(tc.PageMap.ContainsKey(1));
            var c = tc.PageMap[1];
            Assert.AreEqual(typeof(TestRecord), c.RecordType);
            if (lockRules == null)
                Assert.AreEqual(ConsistencyAbilities.None, c.ConsistencyConfiguration.ConsistencyAbilities);
            else
            {
                Assert.AreEqual(ConsistencyAbilities.PhysicalLocks, c.ConsistencyConfiguration.ConsistencyAbilities);
                Assert.AreEqual(lockRules, c.ConsistencyConfiguration.LockRules);
            }
            Assert.IsInstanceOfType(c, typeof(FixedRecordTypePageConfiguration<TestRecord>));
            var c2 = c as FixedRecordTypePageConfiguration<TestRecord>;
            Assert.IsNotNull(c2.RecordMap);
            Assert.AreEqual(recordSize, c2.RecordMap.GetSize);
            var t = new byte[1];
            var t2 = default(TestRecord);
            c2.RecordMap.FillBytes(ref t2, t);
            Assert.AreEqual(1, t[0]);
            c2.RecordMap.FillFromBytes(t, ref t2);
            Assert.AreEqual(2, t[0]);
        }

        private static void VerifyImageRecordCommon(TestingConfig tc)
        {
            Assert.AreEqual(1, tc.PageMap.Count);
            Assert.IsTrue(tc.PageMap.ContainsKey(1));
            var c = tc.PageMap[1];
            Assert.AreEqual(typeof(TestRecord), c.RecordType);
           
                Assert.AreEqual(ConsistencyAbilities.None, c.ConsistencyConfiguration.ConsistencyAbilities);
           
            Assert.IsInstanceOfType(c, typeof(ImageTypePageConfiguration<TestRecord>));
            var c2 = c as ImageTypePageConfiguration<TestRecord>;
            Assert.IsNotNull(c2.RecordMap);
            Assert.AreEqual(c2.PageSize, c2.RecordMap.GetSize);
            var t = new byte[1];
            var t2 = default(TestRecord);
            c2.RecordMap.FillBytes(ref t2, t);
            Assert.AreEqual(1, t[0]);
            c2.RecordMap.FillFromBytes(t, ref t2);
            Assert.AreEqual(2, t[0]);
        }

        [TestMethod]
        public void DefineOnlyNumberTypeAndFixedRecordAlt()
        {
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var definition = A.Fake<IFixedSizeRecordDefinition<TestRecord>>();
            var t2 = default(TestRecord);
            A.CallTo(() => definition.FillBytes(ref t2, null)).WithAnyArguments().AssignsOutAndRefParametersLazily(
                a =>
                {
                    var a0 = (TestRecord)a.Arguments[0];
                    filler(ref a0, a.Arguments[1] as byte[]);
                    return new[] { a0 as object };
                }
                );
            A.CallTo(() => definition.FillFromBytes(null, ref t2)).WithAnyArguments()
                            .AssignsOutAndRefParametersLazily(a =>
                            {
                                var a1 = (TestRecord)a.Arguments[1];
                                getter(a.Arguments[0] as byte[], ref a1);
                                return new[] { a1 as object };
                            });
            A.CallTo(() => definition.Size).Returns(10);
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
            var definition = A.Fake<IFixedSizeRecordDefinition<TestRecord>>();
            var t2 = default(TestRecord);
            A.CallTo(() => definition.FillBytes(ref t2, null)).WithAnyArguments().AssignsOutAndRefParametersLazily(
                            a =>
                            {
                                var a0 = (TestRecord)a.Arguments[0];
                                filler(ref a0, a.Arguments[1] as byte[]);
                                return new[] { a0 as object };
                            }
                            );
            A.CallTo(() => definition.FillFromBytes(null, ref t2)).WithAnyArguments().AssignsOutAndRefParametersLazily(a =>
            {
                var a1 = (TestRecord)a.Arguments[1];
                getter(a.Arguments[0] as byte[], ref a1);
                return new[] { a1 as object };
            });
            A.CallTo(() => definition.Size).Returns(10);
            var locks = A.Fake<LockRuleset>();
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
            var locks = A.Fake<LockRuleset>();
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
            var locks = A.Fake<LockRuleset>();
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
            var c2 = c as PageHeadersConfiguration<TestHeader>;
            Assert.IsNotNull(c2.Header);
            Assert.AreEqual(10, c2.Header.GetSize);
            var t2 = new byte[1];
            var t3 = new TestHeader { Value = 3 };
            c2.Header.FillBytes(ref t3, t2);
            Assert.AreEqual(3, t2[0]);
            c2.Header.FillFromBytes(t2, ref t3);
            Assert.AreEqual(4, t2[0]);
        }

        private static IHeaderDefinition<TestHeader> CommonHeaderDefinition()
        {
            Getter<TestHeader> filler = (ref TestHeader a, byte[] b) => { b[0] = 3; };
            Setter<TestHeader> getter = (byte[] a, ref TestHeader b) => { a[0] = 4; };
            var t = A.Fake<IHeaderDefinition<TestHeader>>();
            var t3 = default(TestHeader);
            A.CallTo(() => t.FillBytes(ref t3, null)).WithAnyArguments().AssignsOutAndRefParametersLazily(
                            a =>
                            {
                                var a0 = (TestHeader)a.Arguments[0];
                                filler(ref a0, a.Arguments[1] as byte[]);
                                return new[] { a0 as object };
                            }
                            );
            A.CallTo(() => t.FillFromBytes(null, ref t3)).WithAnyArguments().AssignsOutAndRefParametersLazily(a =>
            {
                var a1 = (TestHeader)a.Arguments[1];
                getter(a.Arguments[0] as byte[], ref a1);
                return new[] { a1 as object };
            });
            A.CallTo(() => t.Size).Returns(10);
            return t;
        }

        private static IFixedSizeRecordDefinition<TestRecord> CommonFixedRecordDefinition(int size = 10)
        {
            Getter<TestRecord> filler = (ref TestRecord a, byte[] b) => { b[0] = 1; };
            Setter<TestRecord> getter = (byte[] a, ref TestRecord b) => { a[0] = 2; };
            var definition = A.Fake<IFixedSizeRecordDefinition<TestRecord>>();
            var t3 = default(TestRecord);
            A.CallTo(() => definition.FillBytes(ref t3, null)).WithAnyArguments().AssignsOutAndRefParametersLazily(
                            a =>
                            {
                                var a0 = (TestRecord)a.Arguments[0];
                                filler(ref a0, a.Arguments[1] as byte[]);
                                return new[] { a0 as object };
                            }
                            ); ;
            A.CallTo(() => definition.FillFromBytes(null, ref t3)).WithAnyArguments().AssignsOutAndRefParametersLazily(a =>
            {
                var a1 = (TestRecord)a.Arguments[1];
                getter(a.Arguments[0] as byte[], ref a1);
                return new[] { a1 as object };
            });
            A.CallTo(() => definition.Size).Returns(size);
            return definition;
        }
    }
}
