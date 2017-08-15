using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Configurations.Builder;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;

namespace File.Paging.PhysicalLevel.Implementations
{
    internal sealed class LockManager<T>:IPhysicalLockManager<T>
    {
        private class LockHolder
        {
            public volatile int LockInfo;
            public  readonly int[] SelfSharedLocks;
            public long LastUsage;
            public LockHolder(byte selfSharedLocks)
            {
                 SelfSharedLocks = new int[selfSharedLocks];
            }
        }
        private ConcurrentDictionary<T, LockHolder> _locks = new ConcurrentDictionary<T, LockHolder>();

        public bool AcqureLock(T lockingObject, byte lockType, LockMatrix rules, out LockToken<T> token)
        {
            var h = _locks.GetOrAdd(lockingObject, _ => new LockHolder(rules.SelfSharedLocks));
            h.LastUsage = Stopwatch.GetTimestamp();
       
            var entrancePair = rules.EntrancePair(lockType,h.LockInfo);
            if (entrancePair.HasValue)            
            {
                unchecked
                {
                    if (Interlocked.CompareExchange(ref h.LockInfo, (int)entrancePair.Value.BlockExit, (int)entrancePair.Value.BlockEntrance) == (int)entrancePair.Value.BlockEntrance)
                    {
                        if ((h.LockInfo & LockMatrix.SharenessCheckLock) == LockMatrix.SharenessCheckLock)
                        {
                           h.SelfSharedLocks[rules.SelfSharedLockShift(lockType)]++;
                           h.LockInfo = (int)((uint)h.LockInfo & LockMatrix.SharenessCheckLockDrop);
                        }
                        token = new LockToken<T>(lockType,lockingObject,this,rules);
                        return true;
                    }
                }
                
            }
            token = default(LockToken<T>);
            return false;
        }

        public async Task<LockToken<T>> WaitLock(T lockingObject, byte lockType, LockMatrix rules)
        {
            var h = _locks.GetOrAdd(lockingObject, _ => new LockHolder(rules.SelfSharedLocks));
            h.LastUsage = Stopwatch.GetTimestamp();
            return await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var entrancePair = rules.EntrancePair(lockType, h.LockInfo);
                    if (entrancePair.HasValue)
                    {
                        unchecked
                        {
                            if (Interlocked.CompareExchange(ref h.LockInfo, (int) entrancePair.Value.BlockExit,
                                    (int) entrancePair.Value.BlockEntrance) == (int) entrancePair.Value.BlockEntrance)
                            {
                                if ((entrancePair.Value.BlockEntrance & LockMatrix.SharenessCheckLock) ==
                                    LockMatrix.SharenessCheckLock)
                                {
                                    h.SelfSharedLocks[rules.SelfSharedLockShift(lockType)]++;
                                    h.LockInfo = (int) ((uint) h.LockInfo & LockMatrix.SharenessCheckLockDrop);
                                }
                                var token = new LockToken<T>(lockType, lockingObject, this, rules);
                                return token;
                            }
                        }

                    }
                }
                throw new TaskCanceledException();
            }) ;
        }
        
        public void ReleaseLock(LockToken<T> token, LockMatrix rules)
        {
            if (!_locks.TryGetValue(token.LockedObject,out var h))
            {
                throw new InvalidOperationException("Object not locked");
            }
            var lockType = token.LockLevel;

            unchecked
            {
                if (rules.IsSelfShared(lockType))
                {
                    int i;
                    Thread.BeginCriticalRegion();
                    do
                    {
                        i = h.LockInfo;
                    } while (Interlocked.CompareExchange(ref h.LockInfo,i | (int)LockMatrix.SharenessCheckLock,i)!=i);
                    if (--h.SelfSharedLocks[rules.SelfSharedLockShift(lockType)] == -1)
                    {
                        h.LockInfo = (int)rules.ExitPair(lockType,(int)(LockMatrix.SharenessCheckLockDrop & (uint)h.LockInfo)).Value.BlockEntrance;
                    }
                    else
                    {
                        h.LockInfo = h.LockInfo & (int)LockMatrix.SharenessCheckLockDrop;
                    }
                    Thread.EndCriticalRegion();
                }
                else
                {
                     //  foreach (var entrancePair in rules.EntrancePairs(lockType))
                    while (true)
                    {
                        var entrancePair = rules.ExitPair(lockType, h.LockInfo);
                        if (entrancePair.HasValue)
                        {
                            if (Interlocked.CompareExchange(ref h.LockInfo, (int) entrancePair.Value.BlockEntrance,
                                    (int) entrancePair.Value.BlockExit) == (int) entrancePair.Value.BlockExit)
                            {
                                break;
                            }
                        }
                    }

                }
            }

        }
        

        public bool ChangeLockLevel(ref LockToken<T> token, LockMatrix rules, byte newLevel)
        {
            if (!_locks.TryGetValue(token.LockedObject, out var h))
            {
                throw new InvalidOperationException("Object not locked");
            }
            var excalationPair = rules.EscalationPairs(token.LockLevel, newLevel)
                .Select(k=>(LockMatrix.MatrPair?)k)
                .FirstOrDefault(k => k.Value.BlockEntrance == h.LockInfo);
            if (excalationPair.HasValue)
            {
                if (Interlocked.CompareExchange(ref h.LockInfo, (int) excalationPair.Value.BlockExit,
                        (int) excalationPair.Value.BlockEntrance) == excalationPair.Value.BlockEntrance)
                {
                    token = new LockToken<T>(newLevel, token.LockedObject, this, rules);
                    return true;
                }
            }
            return false;
        }

        public Task<LockToken<T>> WaitForLockLevelChange(LockToken<T> token, LockMatrix rules, byte newLevel)
        {
            if (!_locks.TryGetValue(token.LockedObject, out var h))
            {
                throw new InvalidOperationException("Object not locked");
            }
            return Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var excalationPair = rules.EscalationPairs(token.LockLevel, newLevel)
                        .Select(k => (LockMatrix.MatrPair?) k)
                        .FirstOrDefault(k => k.Value.BlockEntrance == h.LockInfo);
                    if (excalationPair.HasValue)
                    {
                        if (Interlocked.CompareExchange(ref h.LockInfo, (int) excalationPair.Value.BlockExit,
                                (int) excalationPair.Value.BlockEntrance) == excalationPair.Value.BlockEntrance)
                        {
                          
                            return new LockToken<T>(newLevel,token.LockedObject,this,rules);
                        }
                    }
                }
            });
        }
    }
}
