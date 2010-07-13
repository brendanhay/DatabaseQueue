using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using DatabaseQueue.Data;
using DatabaseQueue.Extensions;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    public sealed class SqlCompactQueue<T> : AdoNetQueueBase<T>
    {
        private const string CONNECTION = "Data Source=\"{0}\"; Max Database Size=1024; Mode=Exclusive";

        private readonly string _connectionString;

        public SqlCompactQueue(string path, FormatType format, ISerializerFactory<T> factory)
            : this(path, new SqlCompactSchema(format), factory.Create(format)) { }

        public SqlCompactQueue(string path, IStorageSchema schema, ISerializer<T> serializer)
            : base(schema, serializer)
        {
            if (!path.EndsWith(".sdf"))
                throw new ArgumentException("File path must be an .sdf file", "path");

            Path = path;
            CheckTableExists = true;
            _connectionString = string.Format(CONNECTION, path);
        }

        public string Path { get; private set; }

        protected override IDbConnection CreateConnection()
        {
            return new SqlCeConnection(_connectionString);
        }

        #region DatabaseQueue<T> Members

        public override void Initialize()
        {
            var engine = new SqlCeEngine(_connectionString);

            if (!File.Exists(Path))
                engine.CreateDatabase();

            base.Initialize();
        }

        #endregion

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
