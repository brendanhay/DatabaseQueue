using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    public class SynchronizedQueue<T> : IQueue<T>
    {
        private readonly IQueue<T> _queue;

        public SynchronizedQueue(IQueue<T> queue)
        {
            _queue = queue;
        }

        #region IQueue<T> Members

        public int Count
        {
            get
            {
                lock (_queue) 
                    return _queue.Count;
            }
        }

        public bool Synchronized { get { return true; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            lock (_queue)
                return TryEnqueueMultiple(items);
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            lock (_queue)
                return TryDequeueMultiple(out items, max);
        }

        #endregion
    }
}
