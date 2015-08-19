using System;
using System.Collections;
using System.Collections.Generic;

namespace Money.Model
{
    partial class EntitySet<T>
    {
        private readonly List<T> _list;

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            IsDirty = true;
        }

        public void Clear()
        {
            _list.Clear();
            IsDirty = true;
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var removed = _list.Remove(item);
            //important to preserve existing IsDirty
            if (removed)
            {
                IsDirty = true; 
            }
            return removed;
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int RemoveAll(Predicate<T> predicate)
        {
            var count = _list.RemoveAll(predicate);
            //important to preserve existing IsDirty
            if (count > 0)
            {
                IsDirty = true; 
            }
            return count;
        }
    }
}
