using System.Data;

namespace DatabaseQueue.Data
{
    /// <summary>
    /// Represents a column in a database.
    /// Implicit: to int -> StorageColumn.Ordinal
    /// ToString: StorageColumn.Name
    /// </summary>
    public struct StorageColumn
    {
        public readonly int Ordinal;
        public readonly string Name;
        public readonly string SqlType;
        public readonly DbType ParameterType;

        /// <summary>
        /// Create a new StorageColumn
        /// </summary>
        /// <param name="ordinal">Position of the column.</param>
        /// <param name="name">Name of the column.</param>
        /// <param name="sqlType">
        /// Type of the column that will be used in sql queries. 
        /// (ie. int, nvarchar(200), ntext etc.)
        /// </param>
        /// <param name="parameterType">
        /// Type of the column that will be set for any SqlCommandParameters
        /// </param>
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
