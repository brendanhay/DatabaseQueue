using System;
using DatabaseQueue.Data;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// Factory to create IQueue<typeparam name="T"/>s which write data to a backing database.
    /// </summary>
    /// <typeparam name="T">The item type the queue/serializer will support.</typeparam>
    public sealed class DatabaseQueueFactory<T>
    {
        private readonly ISerializerFactory _serializerFactory;

        public DatabaseQueueFactory(ISerializerFactory serializerFactory)
        {
            _serializerFactory = serializerFactory;
        }

        /// <summary>
        /// Creates a new <see cref="IQueue{T}" />
        /// </summary>
        /// <param name="path">Where the database file will be created or opened from.</param>
        /// <param name="database">Database backing the queue.</param>
        /// <param name="format">Format of the data stored in the database.</param>
        /// <returns>
        /// A new <see cref="IQueue{T}" /> or throws a NotSupportedException 
        /// if the database is not supported.
        /// </returns>
        public IQueue<T> Create(string path, DatabaseType database, FormatType format)
        {
            return Create(path, database, format, null);    
        }

        /// <summary>
        /// Creates a new <see cref="IQueue{T}" />
        /// </summary>
        /// <param name="path">Where the database file will be created or opened from.</param>
        /// <param name="database">Database backing the queue.</param>
        /// <param name="format">Format of the data stored in the database.</param>
        /// <param name="performance">
        /// The performance counter to measure item throughput, 
        /// null if performance measurements won't be used.
        /// </param>
        /// <returns>
        /// A new <see cref="IQueue{T}" /> or throws a NotSupportedException 
        /// if the database is not supported.
        /// </returns>
        public IQueue<T> Create(string path, DatabaseType database, FormatType format, 
            IQueuePerformanceCounter performance)
        {
            IQueue<T> queue;

            switch (database)
            {
                case DatabaseType.SqlCompact:
                    queue = new SqlCompactQueue<T>(path, format, _serializerFactory.Create<T>(format), performance);
                    break;
                case DatabaseType.Sqlite:
                    queue = new SqliteQueue<T>(path, format, _serializerFactory.Create<T>(format), performance);
                    break;
                case DatabaseType.Berkeley:
                    queue = new BerkeleyDbQueue<T>(path, _serializerFactory.CreateBinaryComposite<T>(format), performance);
                    break;
                default:
                    throw new NotSupportedException("The DatabaseType you specified is not supported");
            }

            return queue;
        }
    }
}
