using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    public static class SynchronizedQueue
    {
        /// <summary>
        /// Synchronizes and returns a new SynchronizedQueue<typeparamref name="T"/> if the 
        /// supplied queue is not already synchronized, otherwise it returns <param name="queue"/>.
        /// </summary>
        public static IQueue<T> Synchronize<T>(IQueue<T> queue)
        {
            return queue.Synchronized
                ? queue
                : new SynchronizedQueue<T>(queue);
        }
    }
    
    /// <summary>
    /// A synchronized (thread-safe), locking queue.
    /// Synchronized by default (obviously).
    /// </summary>
    /// <typeparam name="T">The item type to be stored in the queue.</typeparam>
    public sealed class SynchronizedQueue<T> : IQueue<T>
    {
        private readonly IQueue<T> _queue;

        /// <summary>
        /// Creates a new SynchronizedQueue<typeparamref name="T"/> 
        /// with a QueueAdapter<typeparamref name="T"/> as the backing store.
        /// </summary>
        public SynchronizedQueue() : this(new QueueAdapter<T>()) { }

        /// <summary>
        /// Creates a new SynchronizedQueue<typeparamref name="T"/> 
        /// with <param name="queue"/> as the backing store.
        /// </summary>
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
