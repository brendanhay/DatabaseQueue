using System.Data;

namespace DatabaseQueue.Data
{
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
