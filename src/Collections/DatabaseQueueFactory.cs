using System;
using DatabaseQueue.Data;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    public sealed class DatabaseQueueFactory<T>
    {
        private readonly ISerializerFactory _serializerFactory;

        public DatabaseQueueFactory(ISerializerFactory serializerFactory)
        {
            _serializerFactory = serializerFactory;
        }

        public IQueue<T> Create(string path, DatabaseType database, FormatType format)
        {
            return Create(path, database, format, null);    
        }

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
