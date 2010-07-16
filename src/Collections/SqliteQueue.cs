using System.Data;
using System.Data.SQLite;
using DatabaseQueue.Data;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Extensions;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// Queue which reads and writes from a Sqlite database
    /// Non-blocking / non-sychronized by default.
    /// Implements: <see cref="AdoNetQueueBase{T}" />
    /// </summary>
    /// <typeparam name="T">The item type the queue/serializer will support.</typeparam>
    internal sealed class SqliteQueue<T> : AdoNetQueueBase<T>
    {
        private const string CONNECTION = "Data Source={0}";

        #region Ctors

        /// <summary>
        /// Creates a new <see cref="SqliteQueue{T}" />.
        /// </summary>
        /// <param name="path">Where the database file will be created or opened from.</param>
        /// <param name="format">Format that will be stored directly in the database.</param>
        /// <param name="serializerFactory">
        /// The factory which will create the serializer to encode items in the database format.
        /// </param>
        public SqliteQueue(string path, FormatType format, ISerializerFactory serializerFactory)
            : this(path, format, serializerFactory, null) { }

        /// <summary>
        /// Creates a new <see cref="SqliteQueue{T}" />.
        /// </summary>
        /// <param name="path">Where the database file will be created or opened from.</param>
        /// <param name="format">Format that will be stored directly in the database.</param>
        /// <param name="serializerFactory">
        /// The factory which will create the serializer to encode items in the database format.
        /// </param>
        /// <param name="performance">
        /// The performance counter to measure item throughput, 
        /// null if performance measurements won't be used.
        /// </param>
        public SqliteQueue(string path, FormatType format, ISerializerFactory serializerFactory,
            IQueuePerformanceCounter performance)
            : this(path, format, serializerFactory.Create<T>(format), performance) { }

        /// <summary>
        /// Creates a new <see cref="SqliteQueue{T}" />.
        /// </summary>
        /// <param name="path">Where the database file will be created or opened from.</param>
        /// <param name="format">Format that will be stored directly in the database.</param>
        /// <param name="serializer">The serializer to encode items in the database format.</param>
        /// <param name="performance">
        /// The performance counter to measure item throughput, 
        /// null if performance measurements won't be used.
        /// </param>
        public SqliteQueue(string path, FormatType format, ISerializer<T> serializer, 
            IQueuePerformanceCounter performance) 
            : this(path, new SqliteSchema(format), serializer, performance) { }

        /// <summary>
        /// Creates a new <see cref="SqliteQueue{T}" />.
        /// </summary>
        /// <param name="path">Where the database file will be created or opened from.</param>
        /// <param name="schema">Schema which defines the database table and layout.</param>
        /// <param name="serializer">The serializer to encode items in the database format.</param>
        /// <param name="performance">
        /// The performance counter to measure item throughput, 
        /// null if performance measurements won't be used.
        /// </param>
        public SqliteQueue(string path, IStorageSchema schema, ISerializer<T> serializer, 
            IQueuePerformanceCounter performance) 
            : base(CreateConnection(path), schema,  serializer, false, performance)
        {
            Path = path;
        }

        #endregion

        public string Path { get; private set; }
    
        private static IDbConnection CreateConnection(string path)
        {
            return new SQLiteConnection(string.Format(CONNECTION, path));
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
