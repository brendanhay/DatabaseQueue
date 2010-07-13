using System.Collections.Generic;
using System.Diagnostics;
using DatabaseQueue.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class BlockingQueueTests
    {
        private const int CAPACITY = 1,
            TIMEOUT = 250;

        private static readonly ICollection<Entity> _items
            = Entity.CreateCollection();

        private IQueue<Entity> _queue;

        [TestInitialize]
        public void TestInitialize()
        {
            _queue = CreateQueue(CAPACITY);
        }

        private static IQueue<Entity> CreateQueue(int capacity)
        {
            return new BlockingQueue<Entity>(new QueueAdapter<Entity>(), capacity, TIMEOUT);
        }

        [TestMethod]
        public void BlockingQueue_TryEnqueueMultiple_EnqueueMoreThanCapacity_BlocksLongerThanTimeout()
        {
            var watch = Stopwatch.StartNew();
            Assert.IsFalse(_queue.TryEnqueueMultiple(_items));
            watch.Stop();

            Assert.IsTrue(watch.ElapsedMilliseconds >= TIMEOUT);
        }

        [TestMethod]
        public void BlockingQueue_TryDequeueMultiple_EmptyQueue_BlocksLongerThanTimeout()
        {
            ICollection<Entity> items;

            var watch = Stopwatch.StartNew();
            Assert.IsFalse(_queue.TryDequeueMultiple(out items, 1));
            watch.Stop();

            Assert.IsTrue(watch.ElapsedMilliseconds >= TIMEOUT);
        }

        [TestMethod]
        public void BlockingQueue_TryDequeueMultiple_MaxGreaterThanAvailable_ReturnsAvailableItems()
        {
            var queue = CreateQueue(_items.Count);
            ICollection<Entity> items;

            Assert.IsTrue(queue.TryEnqueueMultiple(_items));
            Assert.IsTrue(queue.TryDequeueMultiple(out items, _items.Count * 5));
            Assert.IsTrue(items.Count == _items.Count);
        }
    }
}
