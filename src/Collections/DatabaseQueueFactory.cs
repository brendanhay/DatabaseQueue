using System;
using DatabaseQueue.Data;
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
            IQueue<T> queue;

            switch (database)
            {
                case DatabaseType.SqlCompact:
                    queue = new SqlCompactQueue<T>(path, format, _serializerFactory);
                    break;
                case DatabaseType.Sqlite:
                    queue = new SqliteQueue<T>(path, format, _serializerFactory);
                    break;
                case DatabaseType.Berkeley:
                    queue = new BerkeleyDbQueue<T>(path, 
                        _serializerFactory.CreateBinaryComposite<T>(format));
                    break;
                default:
                    throw new NotSupportedException("The DatabaseType you specified is not supported");
            }

            return queue;
        }
    }
}
