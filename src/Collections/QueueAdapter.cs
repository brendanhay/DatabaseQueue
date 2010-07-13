using System;
using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// Maps System.Collections.Generic.Queue[T] to IQueue[T]
    /// </summary>
    public class QueueAdapter<T> : Queue<T>, IQueue<T>
    {
        public QueueAdapter() { }

        public QueueAdapter(IEnumerable<T> items) : base(items) { }

        #region IQueue<T> Members

        public bool Synchronized { get { return false; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
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
            catch (InvalidOperationException)
            {
                // Empty Queue
            }

            return items.Count > 0;
        }

        #endregion
    }
}
