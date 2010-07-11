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

        public BlockingQueue(IQueue<T> queue) : this(queue, -1, 2500) { }

        public BlockingQueue(IQueue<T> queue, int capacity, int timeout)
        {
            _queue = queue;
            _capacity = capacity;
            _timeout = timeout;
        }

        #region Blocking Enqueue / Dequeue

        private bool TryEnqueueMultiple(ICollection<T> items, Func<bool> timer)
        {
            do
            {
                var enqueued = _enqueued;
                var dequeued = _dequeued;

                if (_capacity != -1)
                {
                    if (enqueued - dequeued > _capacity)
                        continue;
                }

                if (Interlocked.CompareExchange(ref _enqueued, enqueued + 1, enqueued) != enqueued)
                    continue;

                if (!_queue.TryEnqueueMultiple(items))
                    throw new InvalidOperationException("The underlying collection didn't accept the item.");

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

                if (Interlocked.CompareExchange(ref _dequeued, dequeued + 1, dequeued) != dequeued)
                    continue;

                return _queue.TryDequeueMultiple(out items, max);

            } while (timer != null && timer());

            return false;
        }

        #endregion

        #region IQueue<T> Members

        public int Count
        {
            get { throw new NotImplementedException(); }
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