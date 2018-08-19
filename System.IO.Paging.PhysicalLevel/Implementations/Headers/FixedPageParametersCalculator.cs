using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace System.IO.Paging.PhysicalLevel.Implementations.Headers
{
    internal class FixedPageParametersCalculator
    {
        private readonly ushort _pageSize;
        private readonly ushort _fixedRecordSize;
        private readonly ushort _bitPerRecordInHeader;
        public virtual int PamSize { get; private set; }
        public virtual int PamIntLength { get; private set; }
        public virtual ushort MaxRecordCount { get; private set; }
        public virtual int LastMask { get; private set; }       
        public virtual int UsedRecords { get; private set; }
        public virtual byte BitsUnusedInLastInt { get; private set; }
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
                    PamIntLength = PamSize / 4 + 1;                 
                    markUsedBecauseOfIntOverhead = (4 - (PamSize % 4)) * 8;
                }
                else
                {
                    PamIntLength = PamSize / 4;               
                    markUsedBecauseOfIntOverhead = 0;
                }
                if (hasByteOverBitsOverhead || hasIntOverhead)            
                {                                     
                    var markUsedBecauseOfBitOverhead = PamSize * 8 - MaxRecordCount * _bitPerRecordInHeader;
                    BitsUnusedInLastInt = (byte)(markUsedBecauseOfBitOverhead + markUsedBecauseOfIntOverhead);
                    LastMask = int.MinValue >> (BitsUnusedInLastInt - 1);                    
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

        public int CalculateUsed(byte[] rawPam)
        {
            UsedRecords = 0;
            for (int i = 0; i < PamSize; i++)
            {
                UsedRecords += _sizes[rawPam[i]];
            }
            return UsedRecords;
        }

        public  unsafe int[] ProcessPam(byte* rawPam)
        {
          
            Debug.Assert(BitConverter.IsLittleEndian, "BitConverter.IsLittleEndian");
            
            var pm = new int[PamIntLength];
            fixed (void* dst = pm)
            {
                Buffer.MemoryCopy(rawPam, dst, PamSize, PamSize);
            }
            return pm;
        }
    }
}
