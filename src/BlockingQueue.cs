using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DatabaseQueue
{
    public class BlockingQueue<T> : IQueue<T>, IDisposable
    {
        private readonly IQueue<T> _queue;
        private readonly int _capacity, 
            _timeout;

        private long _enqueued = long.MinValue, 
            _dequeued = long.MinValue;

        public BlockingQueue(IQueue<T> queue, int capacity, int timeout)
        {
            _queue = queue;
            _capacity = capacity;
            _timeout = timeout;
            _enqueued += queue.Count;
        }

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
                    throw new InvalidOperationException("The underlying collection didn't accept the item.");

                return true;

            } while (timer != null && timer());

            return false;
        }

        // TODO: Fix this decrementing fuckup
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

                return _queue.TryDequeueMultiple(out items, max);

            } while (timer != null && timer());

            return false;
        }

        #endregion

        #region IQueue<T> Members

        public int Count
        {
            get { return _queue.Count; }
        }

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
            throw new NotImplementedException();
        }

        #endregion
    }
}
