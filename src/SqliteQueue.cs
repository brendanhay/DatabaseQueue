using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace DatabaseQueue
{
    #region Factory Methods

    /// <summary>
    /// Some factory method examples of the various queue combinations
    /// </summary>
    public static class SqliteQueue
    {   
        #region Standard 

        public static IQueue<T> CreateBinaryQueue<T>(string path)
        {
            var schema = StorageSchema.Create("integer", DbType.Int32, "blob", DbType.Binary);

            return new SqliteQueue<T>(path, schema, new BinarySerializer<T>());
        }

        public static IQueue<T> CreateJsonQueue<T>(string path)
        {
            var schema = StorageSchema.Create("integer", DbType.Int32, "text", DbType.String);

            return new SqliteQueue<T>(path, schema, new JsonSerializer<T>());
        }

        public static IQueue<T> CreateXmlQueue<T>(string path)
        {
            var schema = StorageSchema.Create("integer", DbType.Int32, "text", DbType.String);

            return new SqliteQueue<T>(path, schema, new XmlSerializer<T>());
        }

        #endregion

        #region Blocking

        public static IQueue<T> CreateBlockingBinaryQueue<T>(string path, int capacity,
            int timeout)
        {
            var queue = CreateBinaryQueue<T>(path);

            return new BlockingQueue<T>(queue, capacity, timeout);
        }

        public static IQueue<T> CreateBlockingJsonQueue<T>(string path, int capacity, int timeout)
        {
            var queue = CreateJsonQueue<T>(path);

            return new BlockingQueue<T>(queue, capacity, timeout);
        }

        public static IQueue<T> CreateBlockingXmlQueue<T>(string path, int capacity, int timeout)
        {
            var queue = CreateXmlQueue<T>(path);

            return new BlockingQueue<T>(queue, capacity, timeout);
        }

        #endregion

        #region ThreadSafe / Blocking

        public static IQueue<T> CreateThreadSafeBlockingBinaryQueue<T>(string path, int capacity, 
            int timeout)
        {
            var queue = CreateBlockingBinaryQueue<T>(path, capacity, timeout);

            return new ThreadSafeQueue<T>(queue);
        }

        public static IQueue<T> CreateThreadSafeBlockingJsonQueue<T>(string path, int capacity,
             int timeout)
        {
            var queue = CreateBlockingJsonQueue<T>(path, capacity, timeout);

            return new ThreadSafeQueue<T>(queue);
        }

        public static IQueue<T> CreateThreadSafeBlockingXmlQueue<T>(string path, int capacity,
             int timeout)
        {
            var queue = CreateBlockingXmlQueue<T>(path, capacity, timeout);

            return new ThreadSafeQueue<T>(queue);
        }

        #endregion
    }

    #endregion

    public sealed class SqliteQueue<T> : DatabaseQueueBase<T>
    {
        #region Formats

        private const string CONNECTION = "Data Source={0}",
            CREATE_TABLE = "CREATE TABLE IF NOT EXISTS {0}({1} {2} primary key autoincrement, {3} {4})",
            SELECT = "SELECT * FROM {0} LIMIT {1}",
            DELETE = "DELETE FROM {0} WHERE {1} IN({2})",
            INSERT = "INSERT INTO {0}({1}) VALUES(?)",
            COUNT = "SELECT COUNT({0}) FROM {1}";

        #endregion

        public SqliteQueue(string path, IStorageSchema schema, ISerializer<T> serializer) 
            : base(schema,  serializer)
        {
            Path = path;
        }

        public string Path { get; private set; }

        #region DatabaseQueue<T> Members

        protected override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(string.Format(CONNECTION, Path));
        }

        protected override IDbCommand CreateInsertCommand(out IDbDataParameter parameter)
        {
            var commandText = string.Format(INSERT, Schema.Table, Schema.Value);
            var command = CreateCommand(commandText);
            
            parameter = command.CreateParameter();
            parameter.DbType = Schema.Value.ParameterType;
            command.Parameters.Add(parameter);

            return command;
        }

        protected override IDbCommand CreateSelectCommand(int max)
        {
            var commandText = string.Format(SELECT, Schema.Table, max);

            return CreateCommand(commandText);
        }

        protected override IDbCommand CreateDeleteCommand(IEnumerable<object> keys)
        {
            var commandText = string.Format(DELETE, Schema.Table, Schema.Key,
                string.Join(",", keys.Select(key => key.ToString()).ToArray()));

            return CreateCommand(commandText);
        }

        protected override IDbCommand CreateCountCommand()
        {
            var commandText = string.Format(COUNT, Schema.Key, Schema.Table);

            return CreateCommand(commandText);
        }

        protected override void EnsureTableExists()
        {
            var commandText = string.Format(CREATE_TABLE, Schema.Table,
                Schema.Key, Schema.Key.SqlType, Schema.Value, Schema.Value.SqlType);

            using (var command = CreateCommand(commandText))
                command.ExecuteNonQuery();
        }

        #endregion

        private IDbCommand CreateCommand(string commandText)
        {
            var command = Connection.CreateCommand();
            command.CommandText = commandText;

            return command;
        }
    }
}
