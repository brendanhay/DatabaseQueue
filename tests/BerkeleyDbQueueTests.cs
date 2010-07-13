using System.Collections.Generic;
using System.Data;
using System.IO;
using DatabaseQueue.Collections;
using DatabaseQueue.Data;
using DatabaseQueue.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class BerkeleyQueueTests
    {
        #region Initialization

        private static readonly ICollection<Entity> _items 
            = Entity.CreateCollection();

        private static BerkeleyQueue<Entity> _queue;
        private static ISerializer<Entity> _serializer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _serializer = new JsonSerializer<Entity>();

            //var path = GetFilePath(context, "BerkeleyQueue.db");
            var path = "d:\\proj\\app\\databasequeue\\BerkeleyQueue.db";
            _queue = new BerkeleyQueue<Entity>(path);
            _queue.Initialize();
        }

        public TestContext TestContext { get; set; }

        #endregion

        [TestMethod]
        public void BerkeleyQueue_Initialize_CreatesFile()
        {
            Assert.IsTrue(File.Exists(_queue.Path));
        }

        [TestMethod]
        public void BerkeleyQueue_TryEnqueueMultiple_IsSucessful()
        {
            Assert.IsTrue(_queue.TryEnqueueMultiple(_items));
        }

        [TestMethod]
        public void BerkeleyQueue_TryDequeueMultiple_IsSucessful()
        {
            ICollection<Entity> items;

            Assert.IsTrue(_queue.TryEnqueueMultiple(_items));
            Assert.IsTrue(_queue.TryDequeueMultiple(out items, _items.Count));
        }
    }
}
