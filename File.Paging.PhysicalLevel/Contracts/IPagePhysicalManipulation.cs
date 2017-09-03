using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Paging.PhysicalLevel.Classes;
using File.Paging.PhysicalLevel.Classes.Pages;
using File.Paging.PhysicalLevel.Events;

namespace File.Paging.PhysicalLevel.Contracts
{
    public interface IPhysicalPageManipulation
    {
        void Flush(params PageReference[] pages);
        void MarkPageToRemoveFromBuffer(PageReference page);

        event PageRemovedFromBufferEventHandler PageRemovedFromBuffer;
        event NewPageCreatedEventHandler PageCreated;
    }
}
