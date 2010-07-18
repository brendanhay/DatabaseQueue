using DatabaseQueue.Benchmark;
using DatabaseQueue.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests.Collections
{
    [TestClass]
    public class BerkeleyDbQueueTests : QueueTestBase
    {
        private static IQueue<Entity> _queue;
        private static string _path;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _path = GetFilePath(context, "BerkeleyDbQueue.db");
            _queue = SynchronizedQueue.Synchronize(new BerkeleyDbQueue<Entity>(_path));
        }

        [TestMethod]
        public void BerkeleyDbQueue_Ctor_CreatesFile()
        {
            Assert_FileExists(_path);
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
