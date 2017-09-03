﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Implementations.Headers
{
    internal class FixedPageParametersCalculator
    {
        private readonly ushort _pageSize;
        private readonly ushort _fixedRecordSize;
        private readonly ushort _bitPerRecordInHeader;
        public virtual int PamSize { get; private set; }
        public virtual ushort MaxRecordCount { get; private set; }
        public virtual int LastMask { get; private set; }
        public virtual int[] PageAllocationMap { get; private set; }
        public virtual int UsedRecords { get; private set; }

        public virtual ushort PageSize => _pageSize;

        public virtual ushort FixedRecordSize => _fixedRecordSize;

        public FixedPageParametersCalculator(ushort pageSize,ushort fixedRecordSize, ushort bitPerRecordInHeader = 1)
        {
            _pageSize = pageSize;
            _fixedRecordSize = fixedRecordSize;
            _bitPerRecordInHeader = bitPerRecordInHeader;
        }

        public void CalculatePageParameters()
        {
            checked
            {                
                MaxRecordCount = (ushort)Math.Truncate((double)_pageSize * 8 / (_fixedRecordSize * 8 + _bitPerRecordInHeader));
                PamSize = (int)Math.Ceiling((double) MaxRecordCount *_bitPerRecordInHeader/8);
                var hasIntOverhead = PamSize % 4 != 0;
                var hasByteOverBitsOverhead = MaxRecordCount % (8.0 / _bitPerRecordInHeader) != 0;
                int markUsedBecauseOfIntOverhead;
                if (hasIntOverhead)
                {
                    PageAllocationMap = new int[PamSize / 4 + 1];
                    markUsedBecauseOfIntOverhead = (4 - (PamSize % 4)) * 8;
                }
                else
                {
                    PageAllocationMap = new int[PamSize / 4];
                    markUsedBecauseOfIntOverhead = 0;
                }
                if (hasByteOverBitsOverhead || hasIntOverhead)            
                {                                     
                    var markUsedBecauseOfBitOverhead = PamSize * 8 - MaxRecordCount * _bitPerRecordInHeader;
                    var countOfBitToMarkUsed = markUsedBecauseOfBitOverhead + markUsedBecauseOfIntOverhead;
                    LastMask = int.MinValue >> (countOfBitToMarkUsed-1);                    
                }
               

              
            }
        }

        private static readonly byte[] _sizes;
        static FixedPageParametersCalculator()
        {
            _sizes = new byte[byte.MaxValue+1];
            for (int i = 0; i < _sizes.Length; i++)
            {
                var bv = new BitVector32(i);
                var used = Enumerable.Range(0, 32).Select(k => bv[1 << k]).Count(k => k);
                _sizes[i] = (byte)used;
            }
        }

        public unsafe void ProcessPam(byte[] rawPam)
        {
            Debug.Assert(rawPam.Length == PamSize, "rawPam.Length == PamSize");
            Debug.Assert(BitConverter.IsLittleEndian, "BitConverter.IsLittleEndian");
            //Array.Resize(ref rawPam, PageAllocationMap.Length * 4);
            //for (var index = 0; index < PageAllocationMap.Length; index++)
            //{
            //    var data = (rawPam[index * 4+3] << 24)
            //             | (rawPam[index * 4 + 2] << 16)
            //             | (rawPam[index * 4 + 1] << 8)
            //             | (rawPam[index * 4 ]);
            //    var bv = new BitVector32(data);
            //    var used = Enumerable.Range(0, 32).Select(k => bv[1 << k]).Count(k => k);
            //    UsedRecords += used;
            //    PageAllocationMap[index] = data;
            //}
            foreach (byte t in rawPam)
            {
                UsedRecords += _sizes[t];
            }

            fixed (void* src = rawPam)
            fixed (void* dst = PageAllocationMap)
            {
                Buffer.MemoryCopy(src, dst, PamSize, PamSize);
            }
        }
    }
}