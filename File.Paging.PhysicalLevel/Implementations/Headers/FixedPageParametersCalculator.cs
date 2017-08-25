using System;
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
                if (PamSize % 4 == 0)
                {
                    PageAllocationMap = new int[PamSize / 4];
                }
                else
                {
                    PageAllocationMap = new int[PamSize / 4 + 1];
                    var countOfBitToMarkUsed = PamSize * 8 - MaxRecordCount * _bitPerRecordInHeader + (4-(PamSize % 4)) * 8;
                    LastMask = int.MinValue >> (countOfBitToMarkUsed-1);                    
                }
               

              
            }
        }

        public void ProcessPam(byte[] rawPam)
        {
            Debug.Assert(rawPam.Length == PamSize,"rawPam.Length == PamSize");
            Debug.Assert(BitConverter.IsLittleEndian,"BitConverter.IsLittleEndian");
            Array.Resize(ref rawPam, PageAllocationMap.Length * 4);
            for (var index = 0; index < PageAllocationMap.Length; index++)
            {
                var data = (rawPam[index * 4+3] << 24)
                         | (rawPam[index * 4 + 2] << 16)
                         | (rawPam[index * 4 + 1] << 8)
                         | (rawPam[index * 4 ]);
                var bv = new BitVector32(data);
                var used = Enumerable.Range(0, 32).Select(k => bv[1 << k]).Count(k => k);
                UsedRecords += used;
                PageAllocationMap[index] = data;
            }

            //fixed (void* src = rawPam)
            //fixed (void* dst = PageAllocationMap)
            //{
            //    Buffer.MemoryCopy(src, dst, PamSize*4, PamSize);
            //}
        }
    }
}
