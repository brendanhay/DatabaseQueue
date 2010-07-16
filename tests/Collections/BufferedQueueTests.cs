using System;
using System.Collections.Generic;
using DatabaseQueue.Benchmark;
using DatabaseQueue.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DatabaseQueue.Tests.Collections
{
    [TestClass]
    public class BufferedQueueTests : QueueTestBase
    {
        private readonly int _ceiling = RandomHelper.GetInt32(30, 80), 
            _floor = RandomHelper.GetInt32(1, 20);

        private IQueue<Entity> _buffer;
        private BufferedQueue<Entity> _queue;

        [TestInitialize]
        public void TestInitialize()
        {
            QueueMock = CreateIQueueMock();

            _buffer = new QueueAdapter<Entity>();
            // Pass autoStart: false to prevent the internal thread starting up for our tests
            _queue = new BufferedQueue<Entity>(QueueMock.Object, _buffer, _floor, _ceiling, false);
        }

        private void ExecuteDoWorkOnce()
        {
            _queue.Stop();
            _queue.DoWork();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BufferedQueue_FloorGreaterThanOrEqualToCeiling_Ctor_Throws_ArgumentException()
        {
            new BufferedQueue<Entity>(CreateIQueueMock().Object, CreateIQueueMock().Object, 6, 5, false);

            Assert.Fail("Expected ArgumentException was not thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BufferedQueue_SameQueueAsBufferAndOverflow_Ctor_Throws_ArgumentException()
        {
            new BufferedQueue<Entity>(QueueMock.Object, QueueMock.Object, _floor, _ceiling, false);

            Assert.Fail("Expected ArgumentException was not thrown");
        }

        [TestMethod]
        public void BufferedQueue_Dispose_Calls_InternalQueues_Dispose()
        {
            var bufferMock = CreateIQueueMock();
            var queue = new BufferedQueue<Entity>(QueueMock.Object, bufferMock.Object, _floor,
                _ceiling, false);

            queue.Dispose();

            QueueMock.Verify(mock => mock.Dispose(), "Failed to call overflow.Dispose");
            bufferMock.Verify(mock => mock.Dispose(), "Failed to call buffer.Dispose");
        }

        [TestMethod]
        public void BufferedQueue_FullBuffer_Stop_Calls_Overflow_TryEnqueueMultiple_All()
        {
            // Enqueue directly into the buffer reference, to avoid setting an enqueued event
            Assert.IsTrue(_buffer.TryEnqueueMultiple(Items), "Failed to enqueue items in buffer");

            ExecuteDoWorkOnce();

            QueueMock.Verify(mock => mock.TryEnqueueMultiple(It.Is<ICollection<Entity>>(c => c.Count == Items.Count)),
                Times.Once(), string.Format("Failed to call overflow.TryEnqueueMultiple"));
        }

        [TestMethod]
        public void BufferedQueue_EmptyBuffer_TryDequeueMultiple_Calls_Overflow_TryDequeueMultiple()
        {
            ICollection<Entity> items;

            Assert.IsTrue(_queue.Count == 0, "queue.Count was expected to be 0");
            Assert.IsFalse(_queue.TryDequeueMultiple(out items, 100), 
                "Successfully dequeued items from queue");

            ExecuteDoWorkOnce();

            QueueMock.Verify(mock => mock.TryDequeueMultiple(out items, _floor), 
                Times.Once(), string.Format("Failed to call buffer.TryDequeueMultiple {0}", _floor));
        }

        [TestMethod]
        public void BufferedQueue_FullBuffer_TryEnqueueMultiple_Calls_Overflow_TryEnqueueMultiple()
        {
            Assert.IsTrue(_queue.TryEnqueueMultiple(Items), "Failed to enqueue items in buffer");

            ExecuteDoWorkOnce();

            // Once for overflow, once for stop
            QueueMock.Verify(mock => mock.TryEnqueueMultiple(It.IsAny<ICollection<Entity>>()), 
                Times.Exactly(2), "Failed to call overflow.TryEnqueueMultiple");
        }

        [TestMethod]
        public void BufferedQueue_TryEnqueueMultiple_IsTrue()
        {
            Assert_TryEnqueueMultiple_IsTrue(_queue);
        }

        [TestMethod]
        public void BufferedQueue_TryEnqueueMultiple_NullItems_IsFalse()
        {
            Assert_TryEnqueueMultiple_NullItems_IsFalse(_queue);
        }

        [TestMethod]
        public void BufferedQueue_TryDequeueMultiple_IsTrue()
        {
            Assert_TryDequeueMultiple_IsTrue(_queue);
        }

        [TestMethod]
        public void BufferedQueue_TryDequeueMultiple_0Max_IsFalse()
        {
            Assert_TryDequeueMultiple_0Max_IsFalse(_queue);
        }
    }
}
