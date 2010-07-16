using System;
using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// Maps System.Collections.Generic.Queue<typeparamref name="T"/> to IQueue<typeparamref name="T"/>.
    /// Non-blocking / non-synchronized by default.
    /// </summary>
    /// <typeparam name="T">The item type to be stored in the queue.</typeparam>
    public sealed class QueueAdapter<T> : Queue<T>, IQueue<T>
    {
        /// <summary>
        /// Creates a new, empty QueueAdapter<typeparamref name="T"/>.
        /// </summary>
        public QueueAdapter() { }

        /// <summary>
        /// Creates a new QueueAdapter<typeparamref name="T"/> and enumerates over <param name="items"/>, adding them to the queue
        /// </summary>
        public QueueAdapter(IEnumerable<T> items) : base(items) { }

        #region IQueue<T> Members

        public bool Synchronized { get { return false; } }

        public object SyncRoot { get { return this; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            if (items == null || items.Count < 1)
                return false;

            foreach (var item in items)
                Enqueue(item);

            return true;
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            items = new List<T>();

            try
            {
                for (var i = 0; i <= max; i++)
                {
                    var item = Dequeue();

                    items.Add(item);
                }
            }
            // Empty Queue
            catch (InvalidOperationException) { }

            return items.Count > 0;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() { }

        #endregion
    }
}
