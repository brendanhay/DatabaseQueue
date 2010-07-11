using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace DatabaseQueue
{
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

        public SqliteQueue(string path) 
            : this(path, StorageSchema.Binary(), new BinarySerializer<T>()) { }

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
