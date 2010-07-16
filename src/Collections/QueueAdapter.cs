using System;
using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// Maps <see cref="System.Collections.Generic.Queue{T}" /> to <see cref="IQueue{T}" />.
    /// Non-blocking / non-synchronized by default.
    /// Implements: <see cref="System.Collections.Generic.Queue{T}" />
    /// </summary>
    /// <typeparam name="T">The item type to be stored in the queue.</typeparam>
    public sealed class QueueAdapter<T> : Queue<T>, IQueue<T>
    {
        /// <summary>
        /// Creates a new, empty <see cref="QueueAdapter{T}" />.
        /// </summary>
        public QueueAdapter() { }

        /// <summary>
        /// Creates a new <see cref="QueueAdapter{T}" /> and enumerates 
        /// over <param name="items"/>, adding them to the queue.
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
