using System;
using System.Collections.Generic;
using System.Threading;
using BerkeleyDB;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    public sealed class BerkeleyDbQueue<T> : IQueue<T>
    {
        private readonly ISerializer<T, byte[]> _serializer;
        private readonly RecnoDatabase _database;
        
        private int _count;

        public BerkeleyDbQueue(string path) : this(path, new BinarySerializer<T>()) { }

        public BerkeleyDbQueue(string path, ISerializer<T, byte[]> serializer)
        {
            _serializer = serializer;
            Path = path;

            var databaseConfig = new RecnoDatabaseConfig { 
                Creation = CreatePolicy.IF_NEEDED,
                CacheSize = new CacheInfo(0, 131072, 1),
                Renumber = false
            };

            _database = RecnoDatabase.Open(path, databaseConfig);
            _count = (int)_database.Stats().nData;
        }

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
            try
            {
                foreach (var item in items)
                {
                    byte[] serialized;

                    if (!_serializer.TrySerialize(item, out serialized))
                        continue;

                    var value = new DatabaseEntry(serialized);

                    _database.Append(value);

                    Interlocked.Increment(ref _count);
                }

                return true;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            items = new List<T>();

            try
            {
                using (var cursor = _database.Cursor())
                {
                    for (var i = 0; i < max; i++)
                    {
                        if (!cursor.MoveNext())
                            break;

                        var value = cursor.Current.Value.Data;

                        T deserialized;

                        if (_serializer.TryDeserialize(value, out deserialized))
                            items.Add(deserialized);

                        cursor.Delete();

                        Interlocked.Decrement(ref _count);
                    }
                }

                return items.Count > 0;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        #endregion
    }
}
