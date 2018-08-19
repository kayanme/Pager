namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
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
