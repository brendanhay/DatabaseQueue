using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// A blocking/size queue which starts blocking when the specified maximum capacity is reached.
    /// Designed to be used stand-alone or to wrap existing queues conforming to <see cref="IQueue{T}" />
    /// Blocking / non-synchronized by default.
    /// Implements: <see cref="IQueue{T}" />
    /// </summary>
    /// <typeparam name="T">The item type to be stored in the queue.</typeparam>
    public sealed class BlockingQueue<T> : IQueue<T>
    {
        private readonly IQueue<T> _queue;
        private readonly int _capacity, _timeout;

        private long _enqueued = long.MinValue, 
            _dequeued = long.MinValue;

        #region Ctors

        /// <summary>
        /// Creates a new <see cref="BlockingQueue{T}" />.
        /// </summary>
        /// <param name="capacity">Number of items in the queue before blocking the caller.</param>
        /// <param name="timeout">Number of milliseconds to block until failure is assumed.</param>
        public BlockingQueue(int capacity, int timeout) 
            : this(new QueueAdapter<T>(), capacity, timeout) { }

        /// <summary>
        /// Creates a new <see cref="BlockingQueue{T}" />.
        /// </summary>
        /// <param name="queue">Used as the internal backing store.</param>
        /// <param name="capacity">Number of items in the queue before blocking the caller.</param>
        /// <param name="timeout">Number of milliseconds to block until failure is assumed.</param>
        public BlockingQueue(IQueue<T> queue, int capacity, int timeout)
        {
            _queue = queue;
            _capacity = capacity;
            _timeout = timeout;
            _enqueued += queue.Count;
        }

        #endregion

        #region Blocking Enqueue / Dequeue

        private bool TryEnqueueMultiple(ICollection<T> items, Func<bool> timer)
        {
            var count = items.Count;

            do
            {
                var enqueued = _enqueued;
                var dequeued = _dequeued;

                if (_capacity != -1)
                {
                    if (count > (_capacity - (enqueued - dequeued)))
                        continue;
                }

                if (Interlocked.CompareExchange(ref _enqueued, enqueued + count, enqueued) != enqueued)
                    continue;

                if (!_queue.TryEnqueueMultiple(items))
                    Debug.Assert(false, "Underlying queue failed to accept the items");

                return true;

            } while (timer != null && timer());

            return false;
        }

        private bool TryDequeueMultiple(out ICollection<T> items, int max, Func<bool> timer)
        {
            items = default(ICollection<T>);

            do
            {
                var dequeued = _dequeued;
                var enqueued = _enqueued;

                if (dequeued == enqueued)
                    continue;

                var min = Math.Min((enqueued - dequeued), max);

                if (Interlocked.CompareExchange(ref _dequeued, dequeued + min, dequeued) != dequeued)
                    continue;

                if (!_queue.TryDequeueMultiple(out items, max))
                    Debug.Assert(false, "Underlying queue failed to dequeue items");

                return items.Count > 0;

            } while (timer != null && timer());

            return false;
        }

        #endregion

        #region IQueue<T> Members

        public int Count
        {
            get { return _queue.Count; }
        }

        public bool Synchronized { get { return false; } }

        public object SyncRoot { get { return _queue.SyncRoot; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            var watch = Stopwatch.StartNew();

            return TryEnqueueMultiple(items, () => watch.ElapsedMilliseconds < _timeout);
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            var watch = Stopwatch.StartNew();

            return TryDequeueMultiple(out items, max, () => watch.ElapsedMilliseconds < _timeout);
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
