using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;

namespace File.Paging.PhysicalLevel.Classes
{
    internal sealed class LockMatrix
    {
        public sealed class MatrPair
        {
            public readonly uint BlockEntrance;
            public readonly uint BlockExit;

            public MatrPair(uint blockEntrance, uint blockExit)
            {
                BlockEntrance = blockEntrance;
                BlockExit = blockExit;
            }

            public override string ToString() => $"{BlockEntrance} -> {BlockExit}";
        }

        private readonly byte[] _lockSelfSharedFlag;

        private readonly MatrPair[][] _lockSwitchMatrix;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSelfShared(byte lockType) => _lockSelfSharedFlag[lockType]!=255;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte SelfSharedLockShift(byte lockType) => _lockSelfSharedFlag[lockType];

        public readonly byte SelfSharedLocks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MatrPair> EntrancePairs(byte acquiringLockType)
        {
          
            foreach (var matrPair in _lockSwitchMatrix[acquiringLockType])
            {
                yield return matrPair;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrPair SinglePair(byte type) => _lockSwitchMatrix[type][0];


        public const uint SharenessCheckLock = 0b10000000000000000000000000000000;
        public const uint SharenessCheckLockDrop = ~SharenessCheckLock;

        

        public  LockMatrix(LockRuleset rules)
        {
            byte selfSharedLockShift = 0;
            _lockSelfSharedFlag = new byte[rules.GetLockLevelCount()];
            _lockSwitchMatrix = new MatrPair[rules.GetLockLevelCount()][];

            for (byte i = 0b0; i < rules.GetLockLevelCount(); i++)
            {
                var possibleSwitches = new List<MatrPair>();
                checked
                {
                    possibleSwitches.Add(new MatrPair(0b0, (uint)(0b0 | (0b1 << i))));
                }
                if (rules.AreShared(i, i))
                {
                    _lockSelfSharedFlag[i] = selfSharedLockShift++;
                    possibleSwitches.Add(new MatrPair((uint)(0b0 | (0b1 << i)), (uint)(SharenessCheckLock | (0b1 << i) )));
                }
                else
                {
                    _lockSelfSharedFlag[i] = 0xFF;
                }
                for (byte j = 0b0; j < rules.GetLockLevelCount(); j++)
                {
                    if (i == j)
                        continue;
                    if (rules.AreShared(j, i))
                    {
                      
                        foreach (var possibleSwitch in possibleSwitches.ToArray())
                        {
                            possibleSwitches.Add(new MatrPair((uint)(SharenessCheckLock | (0b1 << j) | possibleSwitch.BlockEntrance), 
                                                              (uint)(SharenessCheckLock | (0b1 << j) | possibleSwitch.BlockExit)));
                        }
                        possibleSwitches.Add(new MatrPair((uint)(0b0 | (0b1 << j)), (uint)((0b1 << j) | (0b1 << i))));
                    }                    
                }
                
                _lockSwitchMatrix[i] = possibleSwitches.ToArray();
                SelfSharedLocks = selfSharedLockShift;
            }
        }

    }
}
