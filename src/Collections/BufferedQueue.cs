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
        
        private readonly IQueue<T> _overflowQueue, _bufferQueue;
        private readonly int _ceiling, _floor;

        //private long _enqueued = long.MinValue,
        //    _dequeued = long.MinValue;

        private readonly Action[] _disposables;

        private readonly AutoResetEvent _bufferEnqueued = new AutoResetEvent(false);

        /// <summary>
        /// Occurs when the number of items enqueued goes below _bufferMin
        /// </summary>
        private AutoResetEvent _starvationEvent = new AutoResetEvent(false);

        private readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);
        
        public BufferedQueue(IDatabaseQueue<T> overflowQueue, IQueue<T> bufferQueue, int ceiling, 
            int floor) : this(overflowQueue, bufferQueue, floor, ceiling, overflowQueue.Dispose)
        {
            // Hard start our DatabaseQueue, since it's 
            // assigned as an IQueue[T] interface 
            overflowQueue.Initialize();
        }

        public BufferedQueue(IQueue<T> overflowQueue, IQueue<T> bufferQueue, int ceiling, int floor)
            : this(overflowQueue, bufferQueue, floor, ceiling, null) { }

        private BufferedQueue(IQueue<T> overflowQueue, IQueue<T> bufferQueue, int ceiling, 
            int floor, params Action[] disposables)
        {
            // This needs to be synchronized if it's not already as 
            // it will possibly be accessed by two threads
            _overflowQueue = SynchronizedQueue.Synchronize(overflowQueue);
            _bufferQueue = SynchronizedQueue.Synchronize(bufferQueue);

            _ceiling = ceiling;
            _floor = floor;

            // These will be called later by BufferedQueue.Dispose/0
            _disposables = disposables;

            // Count the current number of items in the queue, 
            // to prevent subsequent calls to .Count
            //_enqueued += (bufferQueue.Count + overflowQueue.Count);

            // Prepare the thread, but don't start it
            _thread = new Thread(DoWork);
        }

        public bool IsCompleted { get { return _quitEvent.WaitOne(0, false); } }

        // If stop has been called, we need to flush all to disk
        public void DoWork()
        {
            while (!IsCompleted)
            {
                //_bufferEnqueued.WaitOne();

                // Possibly lock from this point?  
                var count = _bufferQueue.Count;

                var overflow = count - _ceiling;
                var underrun = count - _floor;

                // Overflow (PASSIVE)
                if (overflow > 0)
                {
                    ICollection<T> items;

                    if (_bufferQueue.TryDequeueMultiple(out items, overflow) && items.Count > 0)
                        _overflowQueue.TryEnqueueMultiple(items);
                }
                // Replenish (PASSIVE)
                else if (underrun < 0)
                {
                    ICollection<T> items;

                    if (_overflowQueue.TryDequeueMultiple(out items, underrun) && items.Count > 0)
                        _bufferQueue.TryEnqueueMultiple(items);
                }
            }

            // Starvation (ACTIVE)
        }

        public void Stop()
        {
            _quitEvent.Set();
        }

        #region IDatabaseQueue<T> Members

        public void Initialize()
        {
            _thread.Start();
        }

        #endregion

        #region IQueue<T> Members

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool Synchronized { get { return true; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            var success = _bufferQueue.TryEnqueueMultiple(items);

            if (success)
                _bufferEnqueued.Set();

            return success;
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            var success = _bufferQueue.TryDequeueMultiple(out items, max);

            // Two behaviours:

            // 1) Try dequeue as many from queue as possible, then signal starvation
            // Pro - fastest, simplest
            // Con - don't get requested items, even though there may be more than enough

            // 2) Try dequeue as many as possible topping up with overflow items as needed, from this thread
            // Pro - get maximum available items as not to confuse caller
            // Con - need to halt other thread's access via locking to the overflow queue

            if (!success)
                return false;

            var missing = max - items.Count;

            // Check if force replenish is required
            if (missing > 0)
            {
                ICollection<T> extras;

                if (_overflowQueue.TryDequeueMultiple(out extras, missing))
                {
                    foreach (var extra in extras)
                        items.Add(extra);
                }
            }

            return items.Count > 0;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();

            _thread.Join();

            foreach (var disposable in _disposables)
                disposable();

            _bufferEnqueued.Close();
        }

        #endregion
    }
}
