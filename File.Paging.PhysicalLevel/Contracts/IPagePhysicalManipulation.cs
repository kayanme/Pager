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
    public interface IPagePhysicalManipulation
    {
        void GroupFlush(params IPage[] pages);
        void MarkPageToRemoveFromBuffer(PageReference page);

        event PageRemovedFromBufferEventHandler PageRemovedFromBuffer;
        event NewPageCreatedEventHandler PageCreated;
    }
}
