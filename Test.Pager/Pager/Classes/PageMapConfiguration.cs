using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{

    [Export]
//    [PartNotDiscoverable]
    public class PageManagerConfiguration
    {

        public Dictionary<byte, PageConfiguration> PageMap = new Dictionary<byte, PageConfiguration>();

        public enum PageSize { Kb4 = 4*1024, Kb8 = 8 * 1024 }

        public PageSize SizeOfPage { get; set; }
    }

    public abstract class PageConfiguration
    {

    }

    public class FixedRecordTypePageConfiguration<TRecordType> : PageConfiguration
    {
        public RecordDeclaration<TRecordType> RecordType { get; set; }
    }

    public class VarableRecordTypePageConfiguration : PageConfiguration
    {
        public Dictionary<byte, RecordDeclaration> RecordMap = new Dictionary<byte, RecordDeclaration>();
    }

    public abstract class RecordDeclaration
    {
        public abstract Type ClrType { get; }
        public bool IsVariableLength { get; protected set; }
    }

    public class RecordDeclaration<TRecordType>: RecordDeclaration
    {
        public sealed override Type ClrType => typeof(TRecordType);

        public void FillFromBytes(IList<byte> bytes, TRecordType record) => _fillFromByte(bytes, record);

        public void FillBytes(TRecordType record, IList<byte> bytes) => _fillBytes(record, bytes);

        public int GetSize(TRecordType record) => _sizeGet(record);

        private readonly Action<TRecordType, IList<byte>> _fillBytes;
        private readonly Action<IList<byte>, TRecordType> _fillFromByte;
        private readonly Func<TRecordType, int> _sizeGet;

        public RecordDeclaration(Action<TRecordType, IList<byte>> fillBytes,Action<IList<byte>, TRecordType> fillFromByte,Func<TRecordType,int> sizeGet)
        {
            _fillBytes = fillBytes;
            _fillFromByte = fillFromByte;
            _sizeGet = sizeGet;
            IsVariableLength = false;
        }

        public RecordDeclaration(Action<TRecordType, IList<byte>> fillBytes, Action<IList<byte>, TRecordType> fillFromByte, int size)
        {
            _fillBytes = fillBytes;
            _fillFromByte = fillFromByte;
            _sizeGet = _=>size;
            IsVariableLength = false;
        }
    }

  
}
