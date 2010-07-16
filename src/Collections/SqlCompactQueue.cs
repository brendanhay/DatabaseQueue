﻿using System;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using DatabaseQueue.Data;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Extensions;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    internal sealed class SqlCompactQueue<T> : AdoNetQueueBase<T>
    {
        private const string CONNECTION = "Data Source=\"{0}\"; Max Database Size=1024; Mode=Exclusive";

        #region Ctors

        public SqlCompactQueue(string path, FormatType format, ISerializerFactory serializerFactory)
            : this(path, format, serializerFactory, null) { }

        public SqlCompactQueue(string path, FormatType format, ISerializerFactory serializerFactory,
            IQueuePerformanceCounter performance)
            : this(path, format, serializerFactory.Create<T>(format), performance) { }

        public SqlCompactQueue(string path, FormatType format, ISerializer<T> serializer, 
            IQueuePerformanceCounter performance) 
            : this(path, new SqlCompactSchema(format), serializer, performance) { }

        public SqlCompactQueue(string path, IStorageSchema schema, ISerializer<T> serializer, 
            IQueuePerformanceCounter performance) 
            : base(CreateConnection(path), schema, serializer, true, performance)
        {
            if (!path.EndsWith(".sdf"))
                throw new ArgumentException("File path must be an .sdf file", "path");

            Path = path;
        }

        #endregion

        public string Path { get; private set; }

        private static IDbConnection CreateConnection(string path)
        {
            var connectionString = string.Format(CONNECTION, path);
            var engine = new SqlCeEngine(connectionString);

            if (!File.Exists(path))
                engine.CreateDatabase();

            return new SqlCeConnection(connectionString);
        }

        #region Schema

        private class SqlCompactSchema : StorageSchemaBase
        {
            private const string SELECT = "SELECT TOP ({0}) * FROM {1}";

            public SqlCompactSchema(FormatType format) 
                : base("int", DetermineValueTypes(format, "ntext", "varbinary(8000)"))
            {
                InsertCommandText = "INSERT INTO {0}({1}) VALUES(?)".FormatEx(Table, Value);
                DeleteCommandText = "DELETE FROM {0} WHERE {1} = ?".FormatEx(Table, Key);
                CountCommandText = "SELECT COUNT({0}) FROM {1}".FormatEx(Key, Table);
                TableExistsCommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE (TABLE_NAME = '{0}')"
                    .FormatEx(Table);
                CreateTableCommandText = "CREATE TABLE [{0}]([{1}] {2} IDENTITY(1,1) NOT NULL, [{3}] {4})"
                    .FormatEx(Table, Key, Key.SqlType, Value, Value.SqlType);
            }

            public override string GetSelectCommandText(int max)
            {
                return SELECT.FormatEx(max, Table);
            }
        }

        #endregion
    }
}
