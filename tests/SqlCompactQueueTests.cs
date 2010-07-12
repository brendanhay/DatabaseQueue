using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class SqlCompactQueueTests
    {
        #region Initialization

        private static readonly ICollection<Entity> _items 
            = Entity.CreateCollection();

        private static SqlCompactQueue<Entity> _queue;
        private static IStorageSchema _schema;
        private static ISerializer<Entity> _serializer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _schema = StorageSchema.Create("int", DbType.Int32, "ntext", DbType.String);
            _serializer = new JsonSerializer<Entity>();

            //var path = GetFilePath(context, "SqlCompactQueue.sdf");
            var path = "d:\\proj\\app\\databasequeue\\SqlCompactQueue.sdf";
            _queue = new SqlCompactQueue<Entity>(path, _schema, _serializer);
            _queue.Initialize();
        }

        public TestContext TestContext { get; set; }

        #endregion

        #region Helpers

        private static readonly IDictionary<string, string> _cache
            = new Dictionary<string, string>();

        private static string GetFilePath(TestContext context, string queueName)
        {
            return Path.Combine(context.TestDeploymentDir, queueName);
        }

        private static IDbConnection GetSqliteConnection(string path)
        {
            return new SQLiteConnection(string.Format("Data Source={0}", path));
        }

        #endregion

        [TestMethod]
        public void SqlCompactQueue_Initialize_CreatesFile()
        {
            Assert.IsTrue(File.Exists(_queue.Path));
        }

        // Perhaps try filling a data set and checking column names/types?

        //[TestMethod]
        //public void SqlCompactQueue_Schema_Table_Exists()
        //{
        //    var

        //    var schema = GetTableSchema(_queue.Path, _schema.Table);
        //    var table = string.Format("create table {0}", _schema.Table);

        //    Assert.IsTrue(schema.StartsWith(table, StringComparison.OrdinalIgnoreCase));
        //}

        //[TestMethod]
        //public void SqlCompactQueue_Schema_Contains_AutoincrementingIntegerPrimaryKey()
        //{
        //    var schema = GetTableSchema(_queue.Path, _schema.Table);

        //    Assert.IsTrue(schema.Contains("integer primary key autoincrement"));
        //}

        //[TestMethod]
        //public void SqlCompactQueue_Schema_KeyColumn_Exists_WithCorrectType()
        //{
        //    var schema = GetTableSchema(_queue.Path, _schema.Table);
        //    var key = string.Format("{0} {1}", _schema.Key, _schema.Key.SqlType);

        //    Assert.IsTrue(schema.Contains(key));
        //}

        //[TestMethod]
        //public void SqlCompactQueue_Schema_ValueColumn_Exists_WithCorrectType()
        //{
        //    var schema = GetTableSchema(_queue.Path, _schema.Table);
        //    var value = string.Format("{0} {1}", _schema.Value, _schema.Value.SqlType);

        //    Assert.IsTrue(schema.Contains(value));
        //}

        [TestMethod]
        public void SqlCompactQueue_TryEnqueueMultiple_IsSucessful()
        {
            Assert.IsTrue(_queue.TryEnqueueMultiple(_items));
        }

        [TestMethod]
        public void SqlCompactQueue_TryDequeueMultiple_RemovesItemsFromQueue()
        {
            ICollection<Entity> items;

            Assert.IsTrue(_queue.TryEnqueueMultiple(_items));
            Assert.IsTrue(_queue.TryDequeueMultiple(out items, int.MaxValue));
            Assert.IsFalse(_queue.TryDequeueMultiple(out items, int.MaxValue));
        }

        [TestMethod]
        public void SqlCompactQueue_TryEnqueueMultiple_NullItems_IsFailure()
        {
            Assert.IsFalse(_queue.TryEnqueueMultiple(null));
        }

        [TestMethod]
        public void SqlCompactQueue_TryDequeueMultiple_0Max_Returns_EmptyCollection()
        {
            ICollection<Entity> items;

            Assert.IsFalse(_queue.TryDequeueMultiple(out items, 0));
            Assert.IsTrue(items.IsNullOrEmpty());
        }
    }
}
