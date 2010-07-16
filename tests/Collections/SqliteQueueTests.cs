using DatabaseQueue.Benchmark;
using DatabaseQueue.Collections;
using DatabaseQueue.Data;
using DatabaseQueue.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests.Collections
{
    [TestClass]
    public class SqliteQueueTests : QueueTestBase
    {
        private static SqliteQueue<Entity> _queue;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var serializerFactory = new SerializerFactory();

            var path = GetFilePath(context, "SqliteQueue.sqlite");

            _queue = new SqliteQueue<Entity>(path, FormatType.Json, serializerFactory);
        }

        [TestMethod]
        public void SqliteQueue_Ctor_CreatesFile()
        {
            Assert_FileExists(_queue.Path);
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
