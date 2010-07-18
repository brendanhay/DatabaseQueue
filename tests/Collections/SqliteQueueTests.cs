using DatabaseQueue.Benchmark;
using DatabaseQueue.Collections;
using DatabaseQueue.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests.Collections
{
    [TestClass]
    public class SqliteQueueTests : QueueTestBase
    {
        private static IQueue<Entity> _queue;
        private static string _path;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _path = GetFilePath(context, "SqliteQueue.sqlite");
            _queue = SynchronizedQueue.Synchronize(new SqliteQueue<Entity>(_path, 
                FormatType.Json, SerializerFactory));
        }

        [TestMethod]
        public void SqliteQueue_Ctor_CreatesFile()
        {
            Assert_FileExists(_path);
        }

        [TestMethod]
        public void SqliteQueue_TryEnqueueMultiple_IsTrue()
        {
            Assert_TryEnqueueMultiple_IsTrue(_queue);
        }

        [TestMethod]
        public void SqliteQueue_TryEnqueueMultiple_NullItems_IsFalse()
        {
            Assert_TryEnqueueMultiple_NullItems_IsFalse(_queue);
        }

        [TestMethod]
        public void SqliteQueue_TryDequeueMultiple_IsTrue()
        {
            Assert_TryDequeueMultiple_IsTrue(_queue);
        }

        [TestMethod]
        public void SqliteQueue_TryDequeueMultiple_RemovesItemsFromQueue()
        {
            Assert_TryDequeueMultiple_RemovesItemsFromQueue(_queue);
        }

        [TestMethod]
        public void SqliteQueue_TryDequeueMultiple_0Max_IsFalse()
        {
            Assert_TryDequeueMultiple_0Max_IsFalse(_queue);
        }
    }
}
