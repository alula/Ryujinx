using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Common.Collections
{
    public class ConcurrentLinkedList<T> : IProducerConsumerCollection<T>
    {
        private readonly LinkedList<T> _list = new LinkedList<T>();
        private readonly object _syncRoot = new object();
        private int _count;

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _count;
                }
            }
        }

        public bool IsSynchronized => true;

        public object SyncRoot => _syncRoot;

        public void CopyTo(Array array, int index)
        {
            lock (_syncRoot)
            {
                _list.CopyTo((T[])array, index);
            }
        }

        public void AddFirst(T item)
        {
            lock (_syncRoot)
            {
                _list.AddFirst(item);
                _count++;
            }
        }

        public void AddLast(T item)
        {
            lock (_syncRoot)
            {
                _list.AddLast(item);
                _count++;
            }
        }

        public bool TryAdd(T item)
        {
            lock (_syncRoot)
            {
                _list.AddLast(item);
                _count++;
                return true;
            }
        }

        public bool TryTake(out T item)
        {
            lock (_syncRoot)
            {
                if (_count > 0)
                {
                    item = _list.First.Value;
                    _list.RemoveFirst();
                    _count--;
                    return true;
                }
                else
                {
                    item = default;
                    return false;
                }
            }
        }

        public bool TryPeekFirst(out T item)
        {
            lock (_syncRoot)
            {
                if (_count > 0)
                {
                    item = _list.First.Value;
                    return true;
                }
                else
                {
                    item = default;
                    return false;
                }
            }
        }

        public T[] ToArray()
        {
            lock (_syncRoot)
            {
                return _list.ToArray();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (_syncRoot)
            {
                _list.CopyTo(array, index);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return new List<T>(_list).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
