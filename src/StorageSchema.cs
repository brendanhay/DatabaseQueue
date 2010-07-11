using System.Data;

namespace DatabaseQueue
{ 
    public interface IStorageSchema
    {
        string Table { get; }

        StorageColumn Key { get; }

        StorageColumn Value { get; }
    }

    public sealed class StorageSchema : IStorageSchema
    {
        #region Factory Methods

        public static IStorageSchema Binary()
        {
            return Create("Queue", "integer", DbType.Int32, "blob", DbType.Binary);
        }

        public static IStorageSchema Json()
        {
            return Create("Queue", "integer", DbType.Int32, "text", DbType.String);
        }

        public static IStorageSchema Create(string table, string keyType, DbType keyParameter, 
            string valueType, DbType valueParameter)
        {
            var key = new StorageColumn(0, "Id", keyType, keyParameter);
            var value = new StorageColumn(1, "Value", valueType, valueParameter);

            return new StorageSchema(table, key, value);
        }

        #endregion

        public StorageSchema(string table, StorageColumn key, StorageColumn value)
        {
            Table = table;
            Key = key;
            Value = value;
        }

        public string Table { get; private set; }

        public StorageColumn Key { get; private set; }

        public StorageColumn Value { get; private set; }
    }

    public struct StorageColumn
    {
        public readonly int Ordinal;
        public readonly string Name;
        public readonly string SqlType;
        public readonly DbType ParameterType;

        public StorageColumn(int ordinal, string name, string sqlType, DbType parameterType)
        {
            Ordinal = ordinal;
            Name = name;
            SqlType = sqlType;
            ParameterType = parameterType;
        }

        #region Implicit Conversion

        public static implicit operator int(StorageColumn column)
        {
            return column.Ordinal;
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}
