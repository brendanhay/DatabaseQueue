﻿using System;
using DatabaseQueue.Data;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    public class DatabaseQueueFactory<T>
    {
        private readonly ISerializerFactory<T> _serializerFactory;

        public DatabaseQueueFactory(ISerializerFactory<T> serializerFactory)
        {
            _serializerFactory = serializerFactory;
        }

        public IDatabaseQueue<T> Create(string path, DatabaseType database, FormatType format)
        {
            IDatabaseQueue<T> queue;

            switch (database)
            {
                case DatabaseType.SqlCompact:
                    queue = new SqlCompactQueue<T>(path, format, _serializerFactory);
                    break;
                case DatabaseType.Sqlite:
                    queue = new SqliteQueue<T>(path, format, _serializerFactory);
                    break;
                case DatabaseType.Berkeley:
                    // Currently only supports json serialization (or shit stored as strings)
                    queue = new BerkeleyQueue<T>(path);
                    break;
                default:
                    throw new NotSupportedException("The DatabaseType you specified is not supported");
            }

            return queue;
        }
    }
}
