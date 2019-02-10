using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.IO.Paging.PhysicalLevel.Contracts.Internal;
using System.Threading;

namespace System.IO.Paging.PhysicalLevel.Implementations
{
    [Export(typeof(IUnderlyingFileOperator))]
    internal sealed class UnderyingFileOperator : IUnderlyingFileOperator
    {
        private readonly FileStream _file;
        private MemoryMappedFile _map;

        private readonly ConcurrentDictionary<MemoryMappedFile,int> _oldMaps = new ConcurrentDictionary<MemoryMappedFile,int>();
        private string _mapName;
        private int _extentSize;
        [ImportingConstructor]
        internal UnderyingFileOperator(FileStream file,int extentSize)
        {
            _extentSize = extentSize;
            _file = file ?? throw new ArgumentNullException(nameof(file));
            //_mapName = "PageMap" + Guid.NewGuid();
            _mapName = null;
            _map = MemoryMappedFile.CreateFromFile(_file, _mapName, _file.Length!=0?_file.Length:extentSize , MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true);
            Debug.Assert(_map != null, "_map!=null");
            if (_map == null)
                throw new ArgumentException("_map");
            _oldMaps.TryAdd(_map, 0);
        }

        public long FileSize => _file.Length;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public MemoryMappedFile GetMappedFile(long desiredFileLength)
        {
            if (_disposedValue)
                throw new ObjectDisposedException("fileOperator");
            var extentChange = (int)(desiredFileLength - _file.Length) / _extentSize + ((desiredFileLength - _file.Length) % _extentSize == 0 ? 0 : 1);
            if (extentChange>0)
            {
                AddExtent(extentChange);
            }
            Debug.Assert(_file.Length >= desiredFileLength, "_file.Length >= desiredFileLength");
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
            if (_oldMaps[oldMap] == 0 && oldMap != _map)
            {

                _oldMaps.TryRemove(oldMap, out int i);
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
            while (!_oldMaps.TryUpdate(file, i - 1, i));
            CheckMapForCleaning(file);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Debug.Assert(_map != null, "_map!=null");
                    if (_map == null)
                        throw new ArgumentException("_map");
                    _map.Dispose();
                    _file.Dispose();
                    _lock.Dispose();
                }
                
                _disposedValue = true;
            }
        }

       
         ~UnderyingFileOperator()
        {            
            Dispose(true);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {         
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void AddExtent(int extentCount)
        {
            MemoryMappedFile oldMap;
            try
            {

                _lock.EnterWriteLock();
                //_file.SetLength(_file.Length + Extent.Size * extentCount);
                //_file.Flush();
                //  var map = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.FullControl, HandleInheritability.None);
                //+ _file.Length + Extent.Size * extentCount
                var map = MemoryMappedFile.CreateFromFile(_file,_mapName , _file.Length + _extentSize * extentCount, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,true);
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
