﻿using System.Data;
using System.Data.SQLite;
using DatabaseQueue.Data;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Extensions;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    internal sealed class SqliteQueue<T> : AdoNetQueueBase<T>
    {
        private const string CONNECTION = "Data Source={0}";

        #region Ctors

        public SqliteQueue(string path, FormatType format, ISerializerFactory serializerFactory)
            : this(path, format, serializerFactory, null) { }

        public SqliteQueue(string path, FormatType format, ISerializerFactory serializerFactory,
            IQueuePerformanceCounter performance)
            : this(path, format, serializerFactory.Create<T>(format), performance) { }

        public SqliteQueue(string path, FormatType format, ISerializer<T> serializer, 
            IQueuePerformanceCounter performance) 
            : this(path, new SqliteSchema(format), serializer, performance) { }

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
