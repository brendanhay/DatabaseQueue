using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    public static class SynchronizedQueue
    {
        public static IQueue<T> Synchronize<T>(IQueue<T> queue)
        {
            return queue.Synchronized
                ? queue
                : new SynchronizedQueue<T>(queue);
        }
    }

    public sealed class SynchronizedQueue<T> : IQueue<T>
    {
        private readonly IQueue<T> _queue;

        public SynchronizedQueue() : this(new QueueAdapter<T>()) { }

        public SynchronizedQueue(IQueue<T> queue)
        {
            _queue = queue;
        }

        #region IQueue<T> Members

        public int Count
        {
            get
            {
                lock (_queue.SyncRoot) 
                    return _queue.Count;
            }
        }

        public bool Synchronized { get { return true; } }

        public object SyncRoot { get { return _queue.SyncRoot; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            lock (_queue.SyncRoot)
                return _queue.TryEnqueueMultiple(items);
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            lock (_queue.SyncRoot)
                return _queue.TryDequeueMultiple(out items, max);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _queue.Dispose();
        }

        #endregion
    }
}
