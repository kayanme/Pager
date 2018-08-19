using System;

namespace TimeArchiver.Contracts
{
    internal interface IIndexSearch
    {
        IndexRecord? GetRoot(IDisposable readToken);
        IndexRecord[] GetChildren(IndexRecord parent, IDisposable readToken);
        DataPageRef GetDataRef(IndexRecord record, IDisposable readToken);
        IDisposable ReadBlock();
        
    }
}
