using System.Collections.Generic;
using System.Data;

namespace DatabaseQueue.Data
{ 
    public interface IStorageSchema
    {
        string Table { get; }

        StorageColumn Key { get; }
        StorageColumn Value { get; }

        string GetSelectCommandText(int max);

        string InsertCommandText { get; }
        string DeleteCommandText { get; }
        string CountCommandText { get; }
        string CreateTableCommandText { get; }
        string TableExistsCommandText { get; }
    }

    public abstract class StorageSchemaBase : IStorageSchema
    {
        /// <summary>
        /// Alias for the primary ctor/2 presuming that 
        /// DetermineValueTypes was used to figure out the format
        /// </summary>
        protected StorageSchemaBase(string intSqlType, KeyValuePair<string, DbType> valueType) 
            : this(intSqlType, valueType.Key, valueType.Value) { }

        /// <summary>
        /// Defaults the Table to "Queue", intSqlType/DbType.Int32 "Id", 
        /// and valueSqlType/valueParameterType "Value"
        /// </summary>
        protected StorageSchemaBase(string intSqlType, string valueSqlType, DbType valueParameterType)
        {
            Table = "Queue";
            Key = new StorageColumn(0, "Id", intSqlType, DbType.Int32);
            Value = new StorageColumn(1, "Value", valueSqlType, valueParameterType);
        }

        public string Table { get; protected set; }

        public StorageColumn Key { get; protected set; }
        public StorageColumn Value { get; protected set; }

        public abstract string GetSelectCommandText(int max);

        public string InsertCommandText { get; protected set; }
        public string DeleteCommandText { get; protected set; }
        public string CountCommandText { get; protected set; }
        public string CreateTableCommandText { get; protected set; }
        public string TableExistsCommandText { get; protected set; }

        /// <summary>
        /// Presumes Xml and Json formats will be stored as Strings, and anything else as Binary
        /// </summary>
        protected static KeyValuePair<string, DbType> DetermineValueTypes(FormatType format, 
            string sqlStringType, string sqlBinaryType)
        {
            KeyValuePair<string, DbType> pair;

            switch (format)
            {
                case FormatType.Xml:
                case FormatType.Json:
                    pair = new KeyValuePair<string, DbType>(sqlStringType, DbType.String);
                    break;
                default:
                    pair = new KeyValuePair<string, DbType>(sqlBinaryType, DbType.Binary);
                    break;
            }

            return pair;            
        }
    }
}
