using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DatabaseQueue.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class BufferedQueueTests
    {
        private const int CEILING = 10,
            FLOOR = 5;

        private static readonly ICollection<Entity> _items
            = Entity.CreateCollection(100);

        private BufferedQueue<Entity> _queue;

        private Mock<IQueue<Entity>> _bufferMock, 
            _overflowMock;

        private IQueue<Entity> _buffer,
            _overflow;

        [TestInitialize]
        public void TestInitialize()
        {
            _bufferMock = CreateIQueueMock();
            _overflowMock = CreateIQueueMock();
            _buffer = new QueueAdapter<Entity>();
            _overflow = new QueueAdapter<Entity>();

            _queue = new BufferedQueue<Entity>(_overflow, _buffer, 
                CEILING, FLOOR);
            //_queue.Initialize();
        }

        private static Mock<IQueue<Entity>> CreateIQueueMock()
        {
            ICollection<Entity> items = new List<Entity>();

            var queueMock = new Mock<IQueue<Entity>>();
            queueMock.Setup(mock => mock.TryDequeueMultiple(out items, It.IsAny<int>()))
                .Returns(true)
                .Callback(() => queueMock.Setup(mock => mock.Count).Returns(items.Count));

            return queueMock;
        }

        [TestMethod]
        public void BufferedQueue_TryEnqueueMultiple_ExceedingBuffer_Overflow_TryEnqueueMultiple_GetsCalled()
        {
            Assert.IsTrue(_queue.TryEnqueueMultiple(_items));
            _queue.Stop();
            _queue.DoWork();

        }
    }
}
