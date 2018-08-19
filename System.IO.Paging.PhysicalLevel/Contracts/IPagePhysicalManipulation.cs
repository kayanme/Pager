using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Events;

namespace System.IO.Paging.PhysicalLevel.Contracts
{
    public interface IPhysicalPageManipulation
    {
        void Flush(params PageReference[] pages);
        void MarkPageToRemoveFromBuffer(PageReference page);

        event PageRemovedFromBufferEventHandler PageRemovedFromBuffer;
        event NewPageCreatedEventHandler PageCreated;
    }
}
