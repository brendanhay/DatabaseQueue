using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;

namespace DatabaseQueue
{
    #region Factory Methods

    /// <summary>
    /// Some factory method examples of the various queue combinations
    /// </summary>
    public static class SqlCompactQueue
    {
        #region Standard

        public static IDatabaseQueue<T> CreateBinaryQueue<T>(string path)
        {
            var schema = StorageSchema.Create("int", DbType.Int32, "varbinary(8000)", DbType.Binary);

            return new SqlCompactQueue<T>(path, schema, new BinarySerializer<T>());
        }

        public static IDatabaseQueue<T> CreateJsonQueue<T>(string path)
        {
            var schema = StorageSchema.Create("int", DbType.Int32, "ntext", DbType.String);

            return new SqlCompactQueue<T>(path, schema, new JsonSerializer<T>());
        }

        public static IDatabaseQueue<T> CreateXmlQueue<T>(string path)
        {
            var schema = StorageSchema.Create("int", DbType.Int32, "ntext", DbType.String);

            return new SqlCompactQueue<T>(path, schema, new XmlSerializer<T>());
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

    public sealed class SqlCompactQueue<T> : DatabaseQueueBase<T>
    {
        #region Formats

        private const string CONNECTION = "Data Source=\"{0}\"; Max Database Size=1024; Mode=Exclusive",
            CREATE_TABLE = "CREATE TABLE [{0}]([{1}] {2} IDENTITY(1,1) NOT NULL, [{3}] {4})",
            TABLE_EXISTS = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE (TABLE_NAME = '{0}')",
            SELECT = "SELECT TOP ({1}) * FROM {0}",
            DELETE = "DELETE FROM {0} WHERE {1} = ?",
            INSERT = "INSERT INTO {0}({1}) VALUES(?)",
            COUNT = "SELECT COUNT({0}) FROM {1}";

        #endregion

        private readonly string _connectionString;

        public SqlCompactQueue(string path, IStorageSchema schema, ISerializer<T> serializer)
            : base(schema, serializer)
        {
            if (!path.EndsWith(".sdf"))
                throw new ArgumentException("File path must be an .sdf file", "path");

            Path = path;
            _connectionString = string.Format(CONNECTION, path);
        }

        public string Path { get; private set; }

        #region DatabaseQueue<T> Members

        public override void Initialize()
        {
            var engine = new SqlCeEngine(_connectionString);

            if (!File.Exists(Path))
                engine.CreateDatabase();

            base.Initialize();
        }

        protected override IDbConnection CreateConnection()
        {
            return new SqlCeConnection(_connectionString);
        }

        protected override IDbCommand CreateInsertCommand(out IDbDataParameter valueParameter)
        {
            var commandText = string.Format(INSERT, Schema.Table, Schema.Value);
            var command = CreateCommand(commandText);

            valueParameter = command.CreateParameter();
            valueParameter.DbType = Schema.Value.ParameterType;
            command.Parameters.Add(valueParameter);

            return command;
        }

        protected override IDbCommand CreateSelectCommand(int max)
        {
            var commandText = string.Format(SELECT, Schema.Table, max);

            return CreateCommand(commandText);
        }

        protected override IDbCommand CreateDeleteCommand(out IDbDataParameter keyParameter)
        {
            var commandText = string.Format(DELETE, Schema.Table, Schema.Key);
            var command = CreateCommand(commandText);

            keyParameter = command.CreateParameter();
            keyParameter.DbType = Schema.Key.ParameterType;
            command.Parameters.Add(keyParameter);

            return command;
        }

        protected override IDbCommand CreateCountCommand()
        {
            var commandText = string.Format(COUNT, Schema.Key, Schema.Table);

            return CreateCommand(commandText);
        }

        protected override void EnsureTableExists()
        {
            using (var exists = CreateCommand(string.Format(TABLE_EXISTS, Schema.Table)))
            {
                if ((int)exists.ExecuteScalar() > 0)
                    return;

                var createText = string.Format(CREATE_TABLE, Schema.Table,
                    Schema.Key, Schema.Key.SqlType, Schema.Value, Schema.Value.SqlType);

                using (var create = CreateCommand(createText))
                    create.ExecuteNonQuery();
            }
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
