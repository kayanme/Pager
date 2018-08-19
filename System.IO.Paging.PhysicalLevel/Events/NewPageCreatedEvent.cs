using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Events
{
    public sealed class NewPageCreatedEventArgs:EventArgs
    {
        public PageReference PageReference { get; }
        public byte Type { get; }

        internal NewPageCreatedEventArgs(PageReference pageReference,byte type)
        {
            PageReference = pageReference;
            Type = type;
        }
    }

    public delegate void NewPageCreatedEventHandler(object manager,NewPageCreatedEventArgs args);
}
