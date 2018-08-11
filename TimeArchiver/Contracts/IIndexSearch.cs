using System;

namespace TimeArchiver.Contracts
{
    internal interface IIndexSearch
    {
        IndexRecord? GetRoot();
        IndexRecord[] GetChildren(IndexRecord parent);
        DataPageRef GetDataRef(IndexRecord record);
        IDisposable ReadBlock();
        
    }
}
