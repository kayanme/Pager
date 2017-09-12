using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Pages.Contracts
{
    public interface IBinarySearcher<TRecord>:IDisposable where TRecord : struct
    {
        bool MoveLeft();
        bool MoveRight();
        TypedRecord<TRecord> Current { get; }

        TypedRecord<TRecord> LeftOfCurrent { get; }
        TypedRecord<TRecord> RightOfCurrent { get; }
    }
}
