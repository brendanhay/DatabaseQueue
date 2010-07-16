﻿using System;
using System.Collections.Generic;
using System.Threading;
using BerkeleyDB;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    internal class BerkeleyDbQueue<T> : IQueue<T>
    {
        private readonly ISerializer<T, byte[]> _serializer;
        private readonly IQueuePerformanceCounter _performance;
        private readonly RecnoDatabase _database;

        private int _count;

        #region Ctors

        public BerkeleyDbQueue(string path) : this(path, null) { }

        public BerkeleyDbQueue(string path, IQueuePerformanceCounter performance) 
            : this(path, new BinarySerializer<T>(), performance) { }

        public BerkeleyDbQueue(string path, ISerializer<T, byte[]> serializer, 
            IQueuePerformanceCounter performance)
        {
            _serializer = serializer;
            _performance = performance;
            Path = path;

            var databaseConfig = new RecnoDatabaseConfig
            {
                Creation = CreatePolicy.IF_NEEDED,
                CacheSize = new CacheInfo(0, 131072, 1),
                Renumber = false
            };

            _database = RecnoDatabase.Open(path, databaseConfig);
            _count = (int)_database.Stats().nData;
        }

        #endregion

        public string Path { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            _database.Close();
        }

        #endregion

        #region IQueue<T> Members

        public int Count { get { return _count; } }

        public bool Synchronized { get { return false; } }

        public object SyncRoot { get { return this; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            var success = false;

            try
            {
                foreach (var item in items)
                {
                    var start = DateTime.Now;

                    byte[] serialized;

                    if (!_serializer.TrySerialize(item, out serialized))
                        continue;

                    var value = new DatabaseEntry(serialized);

                    _database.Append(value);

                    Interlocked.Increment(ref _count);

                    if (_performance != null)
                        _performance.Enqueue(true, start, 0);
                }

                success = true;
            }
            catch (Exception ex)
            {

            }

            return success;
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            items = new List<T>();
            var success = false;

            try
            {
                using (var cursor = _database.Cursor())
                {
                    for (var i = 0; i < max; i++)
                    {
                        var start = DateTime.Now;

                        if (!cursor.MoveNext())
                            break;

                        var value = cursor.Current.Value.Data;

                        T deserialized;

                        if (_serializer.TryDeserialize(value, out deserialized))
                            items.Add(deserialized);

                        cursor.Delete();

                        Interlocked.Decrement(ref _count);

                        if (_performance != null)
                            _performance.Dequeue(true, start, 0);
                    }
                }

                success = items.Count > 0;
            }
            catch (Exception ex)
            {

            }

            return success;
        }

        #endregion
    }
}
