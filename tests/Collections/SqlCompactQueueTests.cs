using System;
using DatabaseQueue.Benchmark;
using DatabaseQueue.Collections;
using DatabaseQueue.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests.Collections
{
    [TestClass]
    public class SqlCompactQueueTests : QueueTestBase
    {
        private static IQueue<Entity> _queue;
        private static string _path;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _path = GetFilePath(context, "SqlCompactQueue.sdf");
            _queue = SynchronizedQueue.Synchronize(new SqlCompactQueue<Entity>(_path,
                FormatType.Json, SerializerFactory));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SqlCompactQueue_Ctor_InvalidFileExtension_Throws_ArgumentException()
        {
            new SqlCompactQueue<Entity>("Somenonsense." + RandomHelper.GetString(3), 
                FormatType.Json, SerializerFactory);

            Assert.Fail("Expected ArgumentException was not thrown");
        }

        [TestMethod]
        public void SqlCompactQueue_Ctor_CreatesFile()
        {
            Assert_FileExists(_path);
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
