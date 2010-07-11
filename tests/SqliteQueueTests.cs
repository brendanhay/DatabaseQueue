using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class SqliteQueueTests
    {
        #region Initialization

        private static readonly ICollection<DummyEntity> _items 
            = DummyEntity.CreateCollection();

        private static SqliteQueue<DummyEntity> _queue;
        private static IStorageSchema _schema;
        private static ISerializer<DummyEntity> _serializer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _schema = StorageSchema.Binary();
            _serializer = new JsonSerializer<DummyEntity>();

            var path = GetFilePath(context, "SerializationTests.queue");
            _queue = new SqliteQueue<DummyEntity>(path, _schema, _serializer);
            _queue.Initialize();
        }

        public TestContext TestContext { get; set; }

        #endregion

        #region Helpers

        private static readonly IDictionary<string, string> _cache
            = new Dictionary<string, string>();

        private static string GetTableSchema(string path, string table)
        {
            var key = string.Format("{0}_{1}", path, table);

            if (_cache.ContainsKey(key))
                return _cache[key];

            using (var connection = GetSqliteConnection(path))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT sql FROM 
                        (SELECT * FROM sqlite_master UNION ALL SELECT * FROM sqlite_temp_master)
                        WHERE tbl_name LIKE ?
                        AND type  != 'meta' 
                        AND sql NOT NULL 
                        AND name NOT LIKE 'sqlite_%' 
                        ORDER BY substr(type,2,1), name";

                    var parameter = command.CreateParameter();
                    parameter.Value = table;

                    command.Parameters.Add(parameter);

                    var sql = command.ExecuteScalar().ToString();

                    _cache.Add(key, sql);

                    return sql;
                }
            }
        }

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
        public void SqliteQueue_Initialize_CreatesFile()
        {
            Assert.IsTrue(File.Exists(_queue.Path));
        }

        [TestMethod]
        public void SqliteQueue_Schema_Table_Exists()
        {
            var schema = GetTableSchema(_queue.Path, _schema.Table);
            var table = string.Format("create table {0}", _schema.Table);

            Assert.IsTrue(schema.StartsWith(table, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SqliteQueue_Schema_Contains_AutoincrementingIntegerPrimaryKey()
        {
            var schema = GetTableSchema(_queue.Path, _schema.Table);

            Assert.IsTrue(schema.Contains("integer primary key autoincrement"));
        }

        [TestMethod]
        public void SqliteQueue_Schema_KeyColumn_Exists_WithCorrectType()
        {
            var schema = GetTableSchema(_queue.Path, _schema.Table);
            var key = string.Format("{0} {1}", _schema.Key, _schema.Key.SqlType);

            Assert.IsTrue(schema.Contains(key));
        }

        [TestMethod]
        public void SqliteQueue_Schema_ValueColumn_Exists_WithCorrectType()
        {
            var schema = GetTableSchema(_queue.Path, _schema.Table);
            var value = string.Format("{0} {1}", _schema.Value, _schema.Value.SqlType);

            Assert.IsTrue(schema.Contains(value));
        }

        [TestMethod]
        public void SqliteQueue_TryEnqueueMultiple_IsSucessful()
        {
            Assert.IsTrue(_queue.TryEnqueueMultiple(_items, 0));
        }

        [TestMethod]
        public void SqliteQueue_TryDequeueMultiple_RemovesItemsFromQueue()
        {
            ICollection<DummyEntity> items;

            Assert.IsTrue(_queue.TryEnqueueMultiple(_items, 0));
            Assert.IsTrue(_queue.TryDequeueMultiple(out items, int.MaxValue, 0));
            Assert.IsFalse(_queue.TryDequeueMultiple(out items, int.MaxValue, 0));
        }

        [TestMethod]
        public void SqliteQueue_TryEnqueueMultiple_NullItems_IsFailure()
        {
            Assert.IsFalse(_queue.TryEnqueueMultiple(null, 0));
        }

        [TestMethod]
        public void SqliteQueue_TryDequeueMultiple_0Max_Returns_EmptyCollection()
        {
            ICollection<DummyEntity> items;

            Assert.IsFalse(_queue.TryDequeueMultiple(out items, 0, 0));
            Assert.IsTrue(items.IsNullOrEmpty());
        }
    }
}
