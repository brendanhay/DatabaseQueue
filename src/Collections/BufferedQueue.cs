using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DatabaseQueue.Collections
{
    public class BufferedQueue<T> : IDatabaseQueue<T>
    {
        private readonly Thread _thread;
        private readonly IDatabaseQueue<T> _overflowQueue;
        private readonly IQueue<T> _bufferQueue;
        private readonly int _bufferMax, _bufferMin;

        private long _enqueued = long.MinValue,
            _dequeued = long.MinValue;

        /// <summary>
        /// Occurs when items exceeding _bufferMax are enqueued
        /// </summary>
        private AutoResetEvent _overflowEvent = new AutoResetEvent(false);

        /// <summary>
        /// Occurs when the number of items enqueued goes below _bufferMin
        /// </summary>
        private AutoResetEvent _starvationEvent = new AutoResetEvent(false);

        public BufferedQueue(IDatabaseQueue<T> overflowQueue, IQueue<T> bufferQueue, int bufferMax, 
            int bufferMin)
        {
            overflowQueue.Initialize();

            _overflowQueue = overflowQueue;
            _bufferQueue = bufferQueue;
            _bufferMax = bufferMax;
            _bufferMin = bufferMin;
            _enqueued += (bufferQueue.Count + overflowQueue.Count);
            
            _thread = new Thread(DoWork);
        }

        private void DoWork()
        {

        }

        #region IDatabaseQueue<T> Members

        public void Initialize()
        {
            //_thread.Start();
        }

        #endregion

        #region IQueue<T> Members

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            var count = items.Count;

            var enqueued = _enqueued;
            var dequeued = _dequeued;

            var difference = (int)(_bufferMax - (enqueued - dequeued));

            if (Interlocked.CompareExchange(ref _enqueued, enqueued + count, enqueued) != enqueued)
                return false;

            if (count > difference)
            {
                return _bufferQueue.TryEnqueueMultiple(items.Take(difference).ToList()) 
                    && _overflowQueue.TryEnqueueMultiple(items.Skip(difference).ToList());
            }

            return _bufferQueue.TryEnqueueMultiple(items);
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            var dequeued = _dequeued;
            var enqueued = _enqueued;
            
            var min = (int)Math.Min((enqueued - dequeued), max);

            if (_bufferQueue.TryDequeueMultiple(out items, min))
            {
                var difference = min - items.Count;

                if (difference > 0)
                {
                    ICollection<T> extras;

                    if (_overflowQueue.TryDequeueMultiple(out extras, difference))
                    {
                        foreach (var extra in extras)
                            items.Add(extra);
                    }
                }

                return Interlocked.CompareExchange(ref _dequeued, dequeued + items.Count, dequeued) == dequeued;
            }

            return false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _overflowQueue.Dispose();
        }

        #endregion
    }
}
