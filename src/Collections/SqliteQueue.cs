using System.Data;
using System.Data.SQLite;
using DatabaseQueue.Data;
using DatabaseQueue.Extensions;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    public sealed class SqliteQueue<T> : AdoNetQueueBase<T>
    {
        private const string CONNECTION = "Data Source={0}";

        public SqliteQueue(string path, FormatType format, ISerializerFactory<T> factory)
            : this(path, new SqliteSchema(format), factory.Create(format)) { }

        public SqliteQueue(string path, IStorageSchema schema, ISerializer<T> serializer) 
            : base(schema,  serializer)
        {
            Path = path;
        }

        public string Path { get; private set; }
    
        protected override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(string.Format(CONNECTION, Path));
        }

        #region Schema

        private class SqliteSchema : StorageSchemaBase
        {
            private const string SELECT = "SELECT * FROM {0} LIMIT {1}";

            public SqliteSchema(FormatType format) 
                : base("integer", DetermineValueTypes(format, "text", "blob"))
            {
                InsertCommandText = "INSERT INTO {0}({1}) VALUES(?)".FormatEx(Table, Value);
                DeleteCommandText = "DELETE FROM {0} WHERE {1} = ?".FormatEx(Table, Key);
                CountCommandText = "SELECT COUNT({0}) FROM {1}".FormatEx(Key, Table);
                CreateTableCommandText = "CREATE TABLE IF NOT EXISTS {0}({1} {2} primary key autoincrement, {3} {4})"
                    .FormatEx(Table, Key, Key.SqlType, Value, Value.SqlType);
            }

            public override string GetSelectCommandText(int max)
            {
                return SELECT.FormatEx(Table, max);
            }
        }

        #endregion
    }
}
