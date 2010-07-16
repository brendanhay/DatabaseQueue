using System;
using System.Diagnostics;

namespace DatabaseQueue.Diagnostics
{
    public interface IQueuePerformanceCounter
    {
        void Enqueue(bool result, DateTime start, long size);
        void Dequeue(bool result, DateTime start, long size);
    }

    public sealed class QueuePerformanceCounter : IQueuePerformanceCounter, IDisposable
    {
        public const string CATEGORY = "DatabaseQueue";

        static QueuePerformanceCounter()
        {
            if (PerformanceCounterCategory.Exists(CATEGORY))
                PerformanceCounterCategory.Delete(CATEGORY);
            
            var counters = new CounterCreationDataCollection {
                new CounterCreationData(CounterType.EnqueueTime.ToString(), "Time spent enqueing a block of items to the queue", PerformanceCounterType.AverageCount64),
                new CounterCreationData(CounterType.EnqueueTimeBase.ToString(), "", PerformanceCounterType.AverageBase),
                new CounterCreationData(CounterType.EnqueueRate.ToString(), "Number of items enqueued per second", PerformanceCounterType.RateOfCountsPerSecond64),
                new CounterCreationData(CounterType.EnqueueData.ToString(), "Amount of data enqueued per second", PerformanceCounterType.RateOfCountsPerSecond64),
                new CounterCreationData(CounterType.DequeueTime.ToString(), "Time spent dequeuing a block of items from the queue", PerformanceCounterType.AverageCount64),
                new CounterCreationData(CounterType.DequeueTimeBase.ToString(), "", PerformanceCounterType.AverageBase),
                new CounterCreationData(CounterType.DequeueRate.ToString(), "Amount of data dequeued per second", PerformanceCounterType.RateOfCountsPerSecond64),
                new CounterCreationData(CounterType.DequeueData.ToString(), "Throughput of data per second", PerformanceCounterType.RateOfCountsPerSecond64),
                new CounterCreationData(CounterType.Items.ToString(), "Number of items in the queue", PerformanceCounterType.NumberOfItems64),
            };
        
            PerformanceCounterCategory.Create(CATEGORY, "", PerformanceCounterCategoryType.MultiInstance, counters);
        }

        private readonly PerformanceCounter _enqueueRate;
        private readonly PerformanceCounter _enqueueData;
        private readonly PerformanceCounter _enqueueTime;
        private readonly PerformanceCounter _enqueueTimeBase;
        private readonly PerformanceCounter _dequeueRate;
        private readonly PerformanceCounter _dequeueData;
        private readonly PerformanceCounter _dequeueTime;
        private readonly PerformanceCounter _dequeueTimeBase;
        private readonly PerformanceCounter _items;

        public QueuePerformanceCounter(string instance)
        {
            _enqueueRate = new PerformanceCounter(CATEGORY, CounterType.EnqueueRate.ToString(), instance, false);
            _enqueueData = new PerformanceCounter(CATEGORY, CounterType.EnqueueData.ToString(), instance, false);
            _enqueueTime = new PerformanceCounter(CATEGORY, CounterType.EnqueueTime.ToString(), instance, false);
            _enqueueTimeBase = new PerformanceCounter(CATEGORY, CounterType.EnqueueTimeBase.ToString(), instance, false);
            _dequeueRate = new PerformanceCounter(CATEGORY, CounterType.DequeueRate.ToString(), instance, false);
            _dequeueData = new PerformanceCounter(CATEGORY, CounterType.DequeueData.ToString(), instance, false);
            _dequeueTime = new PerformanceCounter(CATEGORY, CounterType.DequeueTime.ToString(), instance, false);
            _dequeueTimeBase = new PerformanceCounter(CATEGORY, CounterType.DequeueTimeBase.ToString(), instance, false);
            _items = new PerformanceCounter(CATEGORY, CounterType.Items.ToString(), instance, false);
        }

        public void Enqueue(bool result, DateTime start, long size)
        {
            if (!result)
                return;

            _items.Increment();

            var elapsed = DateTime.Now - start;

            _enqueueRate.Increment();
            _enqueueData.IncrementBy(size);
            _enqueueTime.IncrementBy(elapsed.Ticks);
            _enqueueTimeBase.Increment();
        }

        public void Dequeue(bool result, DateTime start, long size)
        {
            if (!result)
                return;

            _items.Decrement();

            var elapsed = DateTime.Now - start;

            _dequeueRate.Increment();
            _dequeueData.IncrementBy(size);
            _dequeueTime.IncrementBy(elapsed.Ticks);
            _dequeueTimeBase.Increment();            
        }

        #region IDisposable Members

        public void Dispose()
        {
            _enqueueRate.Dispose();
            _enqueueData.Dispose();
            _enqueueTime.Dispose();
            _enqueueTimeBase.Dispose();
            _dequeueRate.Dispose();
            _dequeueData.Dispose();
            _dequeueTime.Dispose();
            _dequeueTimeBase.Dispose();
            _items.Dispose();
        }

        #endregion
    }
}
