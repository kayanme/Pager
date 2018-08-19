using System.Collections.Generic;
using System.Linq;

namespace System.IO.Paging.PhysicalLevel.Classes
{
    internal sealed class ConcurrentSortedSet<T> where T:IComparable<T>
    {
        private readonly SortedSet<T> _set;

        

        public ConcurrentSortedSet()
        {
            var comp = Comparer<T>.Create((i1, i2) => i1.CompareTo(i2));
            _set = new SortedSet<T>(comp);
        }

        public bool TryTakeMin(out T minVal)
        {
            lock (_set)
            {
                if (!_set.Any())
                {
                    minVal = default(T);
                    return false;
                }
                minVal = _set.Min;
                _set.Remove(minVal);
                return true;
            }
        }

        public void Add(T item)
        {
            lock (_set)
            {
                _set.Add(item);
            }
        }

    }
}
