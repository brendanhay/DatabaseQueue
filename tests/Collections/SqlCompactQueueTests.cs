using System;
using DatabaseQueue.Collections;
using DatabaseQueue.Data;
using DatabaseQueue.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests.Collections
{
    [TestClass]
    public class SqlCompactQueueTests : QueueTestBase
    {
        private static SerializerFactory _serializerFactory;
        private static SqlCompactQueue<Entity> _queue;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var path = GetFilePath(context, "SqlCompactQueue.sdf");

            _serializerFactory = new SerializerFactory();
            _queue = new SqlCompactQueue<Entity>(path, FormatType.Json, _serializerFactory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SqlCompactQueue_Ctor_InvalidFileExtension_Throws_ArgumentException()
        {
            new SqlCompactQueue<Entity>("Somenonsense." + RandomHelper.GetString(3), 
                FormatType.Json, _serializerFactory);

            Assert.Fail("Expected ArgumentException was not thrown");
        }

        [TestMethod]
        public void SqlCompactQueue_Ctor_CreatesFile()
        {
            Assert_FileExists(_queue.Path);
        }

        [TestMethod]
        public void SqlCompactQueue_TryEnqueueMultiple_IsTrue()
        {
            Assert_TryEnqueueMultiple_IsTrue(_queue);
        }

        [TestMethod]
        public void SqlCompactQueue_TryEnqueueMultiple_NullItems_IsFalse()
        {
            Assert_TryEnqueueMultiple_NullItems_IsFalse(_queue);
        }

        [TestMethod]
        public void SqlCompactQueue_TryDequeueMultiple_IsTrue()
        {
            Assert_TryDequeueMultiple_IsTrue(_queue);
        }

        [TestMethod]
        public void SqlCompactQueue_TryDequeueMultiple_RemovesItemsFromQueue()
        {
            Assert_TryDequeueMultiple_RemovesItemsFromQueue(_queue);
        }

        [TestMethod]
        public void SqlCompactQueue_TryDequeueMultiple_0Max_IsFalse()
        {
            Assert_TryDequeueMultiple_0Max_IsFalse(_queue);
        }
    }
}
