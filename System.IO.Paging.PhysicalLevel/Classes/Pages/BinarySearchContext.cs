using System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Linq;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages
{
    public enum KeyPersistanseType
    {
        Physical,
        Logical,
        Key
    }


    internal sealed class BinarySearchContext<TRecord> : TypedPageBase, IBinarySearcher<TRecord> where TRecord : struct
    {
        private readonly ushort[] _nonFreeheaders;
        private readonly IPageHeaders _headers;
        private readonly IRecordAcquirer<TRecord> _recordGetter;
        private readonly PageReference _pageReference;
        private readonly KeyPersistanseType _keyType;
        private int _lowBorder;
        private int _highBorder;
        private readonly bool _isEmpty;
        public BinarySearchContext(
            IPageHeaders headers,
            IRecordAcquirer<TRecord> recordGetter,
            PageReference pageReference,
            KeyPersistanseType keyType,
            Action actionToClean) : base(pageReference, actionToClean)
        {
            _headers = headers;
            _nonFreeheaders = _headers.NonFreeRecords().ToArray();

            _recordGetter = recordGetter;
            _pageReference = pageReference;
            _keyType = keyType;
            if (_nonFreeheaders.Length == 0)
                _isEmpty = true;
            else
            {
                _lowBorder = -1;
                _highBorder = _nonFreeheaders.Length;
            }
        }

        public TypedRecord<TRecord> Current
        {
            get
            {
                if (_isEmpty)
                    return null;
                var hIndex = CurrentHIndex();
                return RecordByHeaderIndex(hIndex);
            }
        }

        private int CurrentHIndex()
        {
            switch (_highBorder - _lowBorder)
            {
                case 0: throw new ApplicationException();
                case 1: throw new ApplicationException();
                case 2:
                   return _lowBorder + 1;
                    break;
                default:
                   return (_highBorder + _lowBorder) / 2;
                    break;
            }
        
        }

        private TypedRecord<TRecord> RecordByHeaderIndex(int hIndex)
        {
            var header = _nonFreeheaders[hIndex];
            var shift = _headers.RecordShift(header);
            var size = _headers.RecordSize(header);
            var record = _recordGetter.GetRecord(shift, size);
            return new TypedRecord<TRecord>
            {
                Reference = PageRecordReference.CreateReference(_pageReference, header, _keyType),
                Data = record
            };
        }


        public bool MoveLeft()
        {
            if (_isEmpty)
                return false;
            switch (_highBorder - _lowBorder)
            {
                case 0:
                    return false;
                case 1: return false;
                case 2:
                    return false;
                case 3:
                    return false;
                default:
                    _highBorder = (_highBorder + _lowBorder) / 2;
                    return true;
            }

        }

        public bool MoveRight()
        {
            if (_isEmpty)
                return false;
            switch (_highBorder - _lowBorder)
            {
                case 0:
                    return false;
                case 1: return false;
                case 2:
                    return false;
                default:
                    _lowBorder = (_highBorder + _lowBorder) / 2;
                    return true;
            }


        }

        public TypedRecord<TRecord> LeftOfCurrent
        {
            get
            {
                if (_isEmpty)
                    return null;
                var hIndex = CurrentHIndex();
                if (hIndex == 0)
                    return null;
                return RecordByHeaderIndex(hIndex - 1);
            }
        }

        public TypedRecord<TRecord> RightOfCurrent
        {
            get
            {
                if (_isEmpty)
                    return null;
                var hIndex = CurrentHIndex();
                if (hIndex == _nonFreeheaders.Length - 1)
                    return null;
                return RecordByHeaderIndex(hIndex + 1);
            }
        }
    }

}