using System;
using System.Collections.Generic;
using System.Threading;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// A queue with an externally facing buffer and a backing store managed by an internal thread. 
    /// Designed to start overflowing (to the backing store) or replenishing (from the backing store)
    /// when certain thresholds (floor/lower, ceiling/upper) are met.
    /// Designed to be used stand-alone or to wrap existing queues conforming to <see cref="IQueue{T}" />
    /// Non-blocking / Synchronized by default.
    /// </summary>
    /// <typeparam name="T">The item type to be stored in the queue.</typeparam>
    public sealed class BufferedQueue<T> : IQueue<T>
    {
        private readonly IQueue<T> _overflowQueue, _bufferQueue;
        private readonly int _ceiling, _floor;
        private readonly Thread _thread;

        #region Buffer Events

        private readonly AutoResetEvent _enqueuedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _dequeuedEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);
        
        private readonly WaitHandle[] _handles = new WaitHandle[3];

        private enum EventType : byte
        {
            Enqueue = 0,
            Dequeue = 1,
            Stop = 2,
        }

        #endregion

        #region Ctors

        /// <summary>
        /// Creates a new <see cref="BufferedQueue{T}" /> and automatically starts it.
        /// </summary>
        /// <param name="overflowQueue">Used by the internal thread to store overflow items.</param>
        /// <param name="floor">Number of items before the internal thread starts replenishing from overflow -> buffer.</param>
        /// <param name="ceiling">Number of items before the internal thread starts overflowing items from buffer -> overflow.</param>
        public BufferedQueue(IQueue<T> overflowQueue, int floor, int ceiling)
            : this(overflowQueue, new QueueAdapter<T>(), floor, ceiling, true) { }

        /// <summary>
        /// Creates a new <see cref="BufferedQueue{T}" />.
        /// </summary>
        /// <param name="overflowQueue">Used by the internal thread to store overflow items</param>
        /// <param name="floor">Number of items before the internal thread starts replenishing from overflow -> buffer.</param>
        /// <param name="ceiling">Number of items before the internal thread starts overflowing items from buffer -> overflow.</param>
        /// <param name="autoStart">Whether to start the internal thread automatically, if false, this.Start() will need to be used.</param>
        public BufferedQueue(IQueue<T> overflowQueue, int floor, int ceiling, bool autoStart)
            : this(overflowQueue, new QueueAdapter<T>(), floor, ceiling, autoStart) { }

        /// <summary>
        /// Creates a new <see cref="BufferedQueue{T}" />.
        /// </summary>
        /// <param name="overflowQueue">Used by the internal thread to store overflow items.</param>
        /// <param name="bufferQueue">Main buffer that call calls to IQueue[T] will operate on.</param>
        /// <param name="floor">Number of items before the internal thread starts replenishing from overflow -> buffer.</param>
        /// <param name="ceiling">Number of items before the internal thread starts overflowing items from buffer -> overflow.</param>
        /// <param name="autoStart">Whether to start the internal thread automatically, if false, this.Start() will need to be used.</param>
        internal BufferedQueue(IQueue<T> overflowQueue, IQueue<T> bufferQueue,
            int floor, int ceiling, bool autoStart)
        {
            if (floor >= ceiling)
                throw new ArgumentException("floor must be less than ceiling");

            if (overflowQueue == bufferQueue)
                throw new ArgumentException("overflowQueue and bufferQueue cannot be the same object");

            // Setup the event array which will be waited on by the internal thread
            SetupEvents(ref _handles);

            // This needs to be synchronized if it's not already as 
            // it will possibly be accessed by two threads
            _overflowQueue = SynchronizedQueue.Synchronize(overflowQueue);
            _bufferQueue = SynchronizedQueue.Synchronize(bufferQueue);

            _floor = floor;
            _ceiling = ceiling;

            // Prepare the thread, but don't start it
            _thread = new Thread(DoWork);

            if (autoStart)
                Start();
        }

        #endregion

        #region Internal Threading

        private void SetupEvents(ref WaitHandle[] handles)
        {
            handles[(int)EventType.Dequeue] = _dequeuedEvent;
            handles[(int)EventType.Enqueue] = _enqueuedEvent;
            handles[(int)EventType.Stop] = _quitEvent;
        }

        private EventType WaitAny()
        {
            return (EventType)WaitHandle.WaitAny(_handles);
        }

        private void Flush(int count)
        {
            Transfer(_bufferQueue, _overflowQueue, count);
        }

        private void Replenish(int count)
        {
            Transfer(_overflowQueue, _bufferQueue, count);
        }

        private static void Transfer(IQueue<T> from, IQueue<T> to, int count)
        {
            if (count < 1)
                return;

            ICollection<T> items;

            if (from.TryDequeueMultiple(out items, count) && items.Count > 0)
                to.TryEnqueueMultiple(items);
        }

        internal void DoWork()
        {
            while (true)
            {
                // Wait for an event to occur
                var eventType = WaitAny();

                // The current count of buffered items
                var count = _bufferQueue.Count;

                // The amount of available space
                var space = _floor - count;
                var excess = count - _ceiling;

                switch (eventType)
                {
                    case EventType.Stop:
                        // Still got some residue lying around, flush it awaaaaay!
                        Flush(count);
                        return;
                    case EventType.Enqueue:
                        // Some items have been add to the buffer, flush excess to overflow
                        Flush(excess);
                        continue;
                    case EventType.Dequeue:
                        // Some items have been removed from the buffer, try replenish from the overflow
                        Replenish(space);
                        continue;
                    default:
                        continue;
                }
            }
        }

        /// <summary>
        /// Starts the internal thread if not ready started
        /// </summary>
        public void Start()
        {
            if (_thread.ThreadState == ThreadState.Unstarted)
                _thread.Start();
        }

        /// <summary>
        /// Signals the wait handle to exit the main loop
        /// </summary>
        public void Stop()
        {
            _quitEvent.Set();
        }

        #endregion

        #region IQueue<T> Members

        /// <summary>
        /// The total count of items across the buffer and overflow (locks both internal queues)
        /// </summary>
        public int Count
        {
            get { return _bufferQueue.Count + _overflowQueue.Count; }
        }

        public bool Synchronized { get { return true; } }

        public object SyncRoot { get { return _bufferQueue.SyncRoot; } }

        /// <summary>
        /// Enqueues items into the internal buffer and if successful, 
        /// notifies the background thread to check for overflow
        /// </summary>
        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            var success = _bufferQueue.TryEnqueueMultiple(items);

            if (success)
                _enqueuedEvent.Set();

            return success;
        }

        /// <summary>
        /// Dequeues a specified maximum number of items from the buffer and notifies
        /// the background thread to replenish if needed.
        /// </summary>
        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            var success = _bufferQueue.TryDequeueMultiple(out items, max);

            _dequeuedEvent.Set();

            return success;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();

            if (_thread.ThreadState == ThreadState.Running)
                _thread.Join();

            foreach (var handle in _handles)
                handle.Close();

            _bufferQueue.Dispose();
            _overflowQueue.Dispose();
        }

        #endregion
    }
}
