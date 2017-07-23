using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pager.Implementations
{
    [Export(typeof(IUnderlyingFileOperator))]
    internal sealed class UnderyingFileOperator : IUnderlyingFileOperator
    {
        private FileStream _file;
        private MemoryMappedFile _map;

        private ConcurrentDictionary<MemoryMappedFile,int> _oldMaps = new ConcurrentDictionary<MemoryMappedFile,int>();

        [ImportingConstructor]
        internal UnderyingFileOperator(FileStream file)
        {
            _file = file;
            _map = MemoryMappedFile.CreateFromFile(_file, "PageMap"+Guid.NewGuid() , _file.Length!=0?_file.Length:Extent.Size , MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
            _oldMaps.TryAdd(_map, 0);
        }

        public long FileSize => _file.Length;

        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public MemoryMappedFile GetMappedFile(long desiredFileLength)
        {

            if (_file.Length < desiredFileLength)
            {
                AddExtent(1);
            }
            
            try
            {
                _lock.EnterReadLock();
                int i;
                do
                {
                    i = _oldMaps[_map];
                }
                while (!_oldMaps.TryUpdate(_map, i + 1, i));
                return _map;
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }

            
        }

        private void CheckMapForCleaning(MemoryMappedFile oldMap)
        {
            if (_oldMaps[oldMap] == 0)
            {
                int i;
                _oldMaps.TryRemove(oldMap, out i);
                Debug.Assert(i == 0, "i==0");
                oldMap.Dispose();
            }
        }

        public void ReturnMappedFile(MemoryMappedFile file)
        {
            int i;
            do
            {
                i = _oldMaps[file];
            }
            while (!_oldMaps.TryUpdate(_map, i - 1, i));
            CheckMapForCleaning(file);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _map.Dispose();
                    _file.Dispose();
                    GC.SuppressFinalize(this);
                }
                
                disposedValue = true;
            }
        }

       
         ~UnderyingFileOperator()
        {            
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void AddExtent(int extentCount)
        {
            MemoryMappedFile oldMap;
            try
            {

                _lock.EnterWriteLock();
                var map = MemoryMappedFile.CreateFromFile(_file, "PageMap" + _file.Length + Extent.Size * extentCount, _file.Length + Extent.Size * extentCount, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
                _oldMaps.TryAdd(map, 0);
                oldMap = _map;
                _map = map;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            CheckMapForCleaning(oldMap);
        }
        #endregion
    }
}
