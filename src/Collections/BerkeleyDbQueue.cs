using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BerkeleyDB;
using DatabaseQueue.Data;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    public sealed class BerkeleyDbQueue<T> : IQueue<T>
    {
        private readonly ISerializer<T> _serializer;
        private readonly RecnoDatabase _database;
        
        private int _count;

        public BerkeleyDbQueue(string path, FormatType format, ISerializerFactory<T> serializerFactory)
            : this(path, serializerFactory.Create(format)) { }

        public BerkeleyDbQueue(string path, ISerializer<T> serializer)
        {
            _serializer = serializer;
            Path = path;

            var databaseConfig = new RecnoDatabaseConfig { 
                Creation = CreatePolicy.IF_NEEDED,
                CacheSize = new CacheInfo(0, 131072, 1)
            };

            _database = RecnoDatabase.Open("c:\\" + path, databaseConfig);
            _count = (int)_database.Stats().nData;
        }

        public string Path { get; private set; }

        private static byte[] GetBytes(object obj)
        {
            byte[] bytes;

            if (obj is string)
                bytes = Encoding.UTF8.GetBytes((string)obj);
            else if (obj is byte[])
                bytes = (byte[])obj;
            else
                throw new ArgumentException("Serialized objects other than byte[] or string are not supported");

            return bytes;
        }

        private static string GetEntry(DatabaseEntry entry)
        {
            var decode = new ASCIIEncoding();

            return decode.GetString(entry.Data);
        }

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
                    object serialized;

                    if (!_serializer.TrySerialize(item, out serialized))
                        continue;

                    var value = new DatabaseEntry(GetBytes(serialized));

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

            using (var cursor = _database.Cursor())
            {
                for (var i = 0; i < max; i++)
                {
                    if (!cursor.MoveNext())
                        continue;

                    var value = GetEntry(cursor.Current.Value);

                    T deserialized;
                    
                    if (_serializer.TryDeserialize(value, out deserialized))
                        items.Add(deserialized);

                    cursor.Delete();

                    Interlocked.Decrement(ref _count);
                }
            }

            return items.Count > 0;
        }

        #endregion
    }
}
