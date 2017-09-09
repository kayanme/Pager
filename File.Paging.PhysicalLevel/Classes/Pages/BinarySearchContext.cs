using System;
using System.Linq;
using File.Paging.PhysicalLevel.Classes.Pages.Contracts;
using File.Paging.PhysicalLevel.Contracts;
using File.Paging.PhysicalLevel.Contracts.Internal;

namespace File.Paging.PhysicalLevel.Classes.Pages
{
  public enum KeyPersistanseType { Physical,Logical,Key}
    

        internal sealed class BinarySearchContext<TRecord> :TypedPageBase, IBinarySearcher<TRecord> where TRecord : struct
        {
            private readonly ushort[] _nonFreeheaders;
            private readonly IPageHeaders _headers;
            private readonly IRecordAcquirer<TRecord> _recordGetter;
            private readonly PageReference _pageReference;
            private readonly KeyPersistanseType _keyType;
            private int _lowBorder;
            private int _highBorder;
            public BinarySearchContext( 
                IPageHeaders headers,
                IRecordAcquirer<TRecord> recordGetter,
                PageReference pageReference,
                KeyPersistanseType keyType,
                Action actionToClean):base(pageReference,actionToClean)
            {
                _headers = headers;
                _nonFreeheaders = _headers.NonFreeRecords().ToArray();
                
                _recordGetter = recordGetter;
                _pageReference = pageReference;
                _keyType = keyType;
                _lowBorder =-1;
                _highBorder = _nonFreeheaders.Length;            
            }

            public TypedRecord<TRecord> Current
            {
                get
                {
                    int hIndex;
                    switch (_highBorder - _lowBorder)
                    {
                        case 0: return null;
                        case 1: return null;
                        case 2: hIndex = _lowBorder + 1;
                            break;
                        default: hIndex = (_highBorder + _lowBorder) / 2;
                            break;
                    }                    
                    
                    var header = _nonFreeheaders[hIndex];
                    var shift = _headers.RecordShift(header);
                    var size = _headers.RecordSize(header);
                    var record = _recordGetter.GetRecord(shift, size);
                    return new TypedRecord<TRecord>
                    {
                       Reference = PageRecordReference.CreateReference(_pageReference,header,_keyType),
                       Data = record
                    };
                }
            }


            public bool MoveLeft()
            {
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

      
        }
    
}