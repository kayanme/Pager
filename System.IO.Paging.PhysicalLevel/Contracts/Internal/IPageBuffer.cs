using System.IO.Paging.PhysicalLevel.Classes;
using System.IO.Paging.PhysicalLevel.Classes.References;
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Events;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    internal interface IPageBuffer:IDisposable
    {        
        BufferedPage GetPageFromBuffer(PageReference pageNum, PageManagerConfiguration _config, int pageSize,int extentSize);
        void MarkPageToRemoveFromBuffer(PageReference pageNum);
        void RemovePageFromBuffer(PageReference page);
        void Flush(params PageReference[] pages);
        void ReleasePageUseAndCleanIfNeeded(PageReference reference, BufferedPage bufferPage);

        event PageRemovedFromBufferEventHandler PageRemovedFromBuffer;
        event NewPageCreatedEventHandler PageCreated;
    }
}