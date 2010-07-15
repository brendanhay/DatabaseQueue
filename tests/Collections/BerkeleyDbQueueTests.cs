using DatabaseQueue.Collections;
using DatabaseQueue.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests.Collections
{
    [TestClass]
    public class BerkeleyDbQueueTests : QueueTestBase
    {
        private static BerkeleyDbQueue<Entity> _queue;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var path = GetFilePath(context, "BerkeleyDbQueue.db");

            _queue = new BerkeleyDbQueue<Entity>(path, new JsonSerializer<Entity>());
        }

        [TestMethod]
        public void BerkeleyDbQueue_Ctor_CreatesFile()
        {
            Assert_FileExists(_queue.Path);
        }

        [TestMethod]
        public void BerkeleyDbQueue_TryEnqueueMultiple_IsTrue()
        {
            Assert_TryEnqueueMultiple_IsTrue(_queue);
        }

        [TestMethod]
        public void BerkeleyDbQueue_TryEnqueueMultiple_NullItems_IsFalse()
        {
            Assert_TryEnqueueMultiple_NullItems_IsFalse(_queue);
        }

        [TestMethod]
        public void BerkeleyDbQueue_TryDequeueMultiple_IsTrue()
        {
            Assert_TryDequeueMultiple_IsTrue(_queue);
        }

        // TODO: some memory issue here
        [TestMethod]
        public void BerkeleyDbQueue_TryDequeueMultiple_RemovesItemsFromQueue()
        {
            Assert_TryDequeueMultiple_RemovesItemsFromQueue(_queue);
        }

        [TestMethod]
        public void BerkeleyDbQueue_TryDequeueMultiple_0Max_IsFalse()
        {
            Assert_TryDequeueMultiple_0Max_IsFalse(_queue);
        }
    }
}
