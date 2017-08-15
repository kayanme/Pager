using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Test.Pager.Locks
{
    [TestClass]
    public class MatrixBuild
    {
        [TestMethod]
        public void SimpleLockRule()
        {
            var r = MockRepository.GenerateStub<LockRuleset>();
            r.Stub(k => k.GetLockLevelCount()).Return(1);
            r.Stub(k => k.AreShared(0, 0)).Return(false);

            var matrix = new LockMatrix(r);

            Assert.IsFalse(matrix.IsSelfShared(0));
            var res = matrix.EntrancePairs(0).ToArray();
            var exp = new[]{ new LockMatrix.MatrPair(0,0x00000001)};
            CollectionAssert.AreEquivalent( exp, res);

            res = matrix.EscalationPairs(0,0).ToArray();
            exp = new LockMatrix.MatrPair[0];
            CollectionAssert.AreEquivalent(exp, res);
        }


        [TestMethod]
        public void SimpleLockRule_WithSelfShared()
        {
            var r = MockRepository.GenerateStub<LockRuleset>();
            r.Stub(k => k.GetLockLevelCount()).Return(1);
            r.Stub(k => k.AreShared(0, 0)).Return(true);

            var matrix = new LockMatrix(r);

            Assert.IsTrue(matrix.IsSelfShared(0));
            var res = matrix.EntrancePairs(0).ToArray();
            var exp = new[] { new LockMatrix.MatrPair(0, 0b1), new LockMatrix.MatrPair(0b1, LockMatrix.SharenessCheckLock|0b1) };
            CollectionAssert.AreEquivalent(exp, res);
        }


        [TestMethod]
        public void Reader_Writer_LockScheme()
        {
            var r = MockRepository.GenerateStub<LockRuleset>();
            r.Stub(k => k.GetLockLevelCount()).Return(2);
            r.Stub(k => k.AreShared(0, 0)).Return(true);
            r.Stub(k => k.AreShared(1, 0)).Return(false);
            r.Stub(k => k.AreShared(0, 1)).Return(false);
            r.Stub(k => k.AreShared(1, 1)).Return(false);

            var matrix = new LockMatrix(r);

            Assert.IsTrue(matrix.IsSelfShared(0));          
            var res = matrix.EntrancePairs(0).ToArray();
            var exp = new[] { new LockMatrix.MatrPair(0, 0b1), new LockMatrix.MatrPair(0b1, LockMatrix.SharenessCheckLock | 0b1) };
            CollectionAssert.AreEquivalent(exp, res);

            Assert.IsFalse(matrix.IsSelfShared(1));
            res = matrix.EntrancePairs(1).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0, 0b10) };
            CollectionAssert.AreEquivalent(exp, res);

            res = matrix.EscalationPairs(0,1).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0b1, 0b10) };
            CollectionAssert.AreEquivalent(exp, res);

            res = matrix.EscalationPairs(1, 0).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0b10, 0b1) };
            CollectionAssert.AreEquivalent(exp, res);

        }

        [TestMethod]
        public void ThreeLocks_TwoPairsShared_OneUnshared()
        {
            var r = MockRepository.GenerateStub<LockRuleset>();
            r.Stub(k => k.GetLockLevelCount()).Return(3);
            r.Stub(k => k.AreShared(0, 0)).Return(false);
            r.Stub(k => k.AreShared(1, 0)).Return(true);
            r.Stub(k => k.AreShared(0, 1)).Return(true);
            r.Stub(k => k.AreShared(1, 1)).Return(false);
            r.Stub(k => k.AreShared(1, 2)).Return(true);
            r.Stub(k => k.AreShared(2, 1)).Return(true);
            r.Stub(k => k.AreShared(2, 2)).Return(false);
            r.Stub(k => k.AreShared(0, 2)).Return(false);
            r.Stub(k => k.AreShared(2, 0)).Return(false);

            var matrix = new LockMatrix(r);

            Assert.IsFalse(matrix.IsSelfShared(0));
            var res = matrix.EntrancePairs(0).ToArray();
            var exp = new[] { new LockMatrix.MatrPair(0,    0b001),
                              new LockMatrix.MatrPair(0b10, 0b011) };
            CollectionAssert.AreEquivalent(exp, res);

            Assert.IsFalse(matrix.IsSelfShared(1));
            res = matrix.EntrancePairs(1).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0,     0b010),
                          new LockMatrix.MatrPair(0b001, 0b011),
                          new LockMatrix.MatrPair(0b100, 0b110)};
            CollectionAssert.AreEquivalent(exp, res);

            Assert.IsFalse(matrix.IsSelfShared(2));
            res = matrix.EntrancePairs(2).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0, 0b100),
                new LockMatrix.MatrPair(0b010, 0b110)};
            CollectionAssert.AreEquivalent(exp, res);

            res = matrix.EscalationPairs(0,1).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0b1, 0b10)};
            CollectionAssert.AreEquivalent(exp, res);

            res = matrix.EscalationPairs(1, 2).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0b10, 0b100)};
            CollectionAssert.AreEquivalent(exp, res);


            res = matrix.EscalationPairs(0, 2).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0b1, 0b100), new LockMatrix.MatrPair(0b11, 0b110) };
            CollectionAssert.AreEquivalent(exp, res);

            res = matrix.EscalationPairs(2, 0).ToArray();
            exp = new[] { new LockMatrix.MatrPair(0b100, 0b1), new LockMatrix.MatrPair(0b110, 0b11) };
            CollectionAssert.AreEquivalent(exp, res);

        }

    }
}
