using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BerkeleyDB;
using DatabaseQueue.Data;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    public sealed class BerkeleyQueue<T> : IQueue<T>
    {
        private readonly ISerializer<T> _serializer;

        private DatabaseEnvironment _environment;
        private readonly BTreeDatabase _database;
        private readonly Sequence _sequence;

        private int _count;

        public BerkeleyQueue(string path, FormatType format, ISerializerFactory<T> serializerFactory)
            : this(path, serializerFactory.Create(format)) { }
  
        public BerkeleyQueue(string path, ISerializer<T> serializer)
        {
            _serializer = serializer;
            Path = path;

            var cacheInfo = new CacheInfo(0, 131072, 1);

            /* Currently environtment + transactions is producing 2000 ms+ inserts 
             * the transaction wrapper class used in enqueue handles this if null or 
             * transactions aren't set
             * 
            var environmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                AutoCommit = true,
                UseLogging = false,
                UseMPool = true,
                UseTxns = true
            };

            _environment = DatabaseEnvironment.Open(Environment.CurrentDirectory,
                environmentConfig);
            _environment.CacheSize = cacheInfo;
             * 
             */

            var databaseConfig = new BTreeDatabaseConfig
            {
                Creation = CreatePolicy.IF_NEEDED,
                //Env = _environment, 
                CacheSize = cacheInfo,
            };

            _database = BTreeDatabase.Open(Path, databaseConfig);

            var sequenceConfig = new SequenceConfig
            {
                BackingDatabase = _database,
                Creation = CreatePolicy.IF_NEEDED,
                Increment = true,
                InitialValue = Int64.MaxValue,
                Wrap = true,
                key = new DatabaseEntry()
            };

            SetEntry(sequenceConfig.key, "berkeleyqueue");
            sequenceConfig.SetRange(Int64.MinValue, Int64.MaxValue);

            _sequence = new Sequence(sequenceConfig);
            _count = (int)_database.Stats().nData - 1;
        }

        public string Path { get; private set; }

        private static void SetEntry(DatabaseEntry entry, object obj)
        {
            byte[] bytes;

            if (obj is string)
                bytes = Encoding.UTF8.GetBytes((string)obj);
            else if (obj is byte[])
                bytes = (byte[])obj;
            else
                throw new ArgumentException("Serialized objects other than byte[] or string are not supported");

            entry.Data = bytes;
        }

        private static string GetEntry(DatabaseEntry entry)
        {
            var decode = new ASCIIEncoding();

            return decode.GetString(entry.Data);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sequence.Close();
            _database.Close();
            
            if (_environment != null)
                _environment.Close();
        }

        #endregion

        #region IQueue<T> Members

        public int Count { get { return _count; } }

        public bool Synchronized { get { return false; } }

        public object SyncRoot { get { return this; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            var transaction = new BerkeleyTransaction(_database, _sequence, _environment);

            try
            {
                foreach (var item in items)
                {
                    object serialized;

                    if (!_serializer.TrySerialize(item, out serialized))
                        continue;

                    var key = new DatabaseEntry();
                    var value = new DatabaseEntry();

                    SetEntry(key, transaction.GetSequence().ToString());
                    SetEntry(value, serialized);

                    transaction.Put(key, value);

                    Interlocked.Increment(ref _count);
                }

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Discard();
            }

            return false;
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            items = new List<T>();

            using (var cursor = _database.Cursor())
            {
                T deserialized;

                /* Walk through the database and print out key/data pairs. */
                for (var i = 0; i < max; i++)
                {
                    if (!cursor.MoveNext())
                        break;

                    var key = cursor.Current.Key;
                    var value = cursor.Current.Value;

                    if (_serializer.TryDeserialize(GetEntry(value), out deserialized))
                        items.Add(deserialized);

                    _database.Delete(key);

                    Interlocked.Decrement(ref _count);
                }
            }

            return items.Count > 0;
        }

        #endregion

        #region Wrappers

        private class BerkeleyTransaction
        {
            private readonly Database _database;
            private readonly Sequence _sequence;
            private readonly Transaction _transaction;

            public BerkeleyTransaction(Database database, Sequence sequence, DatabaseEnvironment environment)
            {
                _database = database;
                _sequence = sequence;

                if (environment != null && environment.UsingTxns)
                    _transaction = environment.BeginTransaction();
            }

            private bool Enabled { get { return _transaction != null; } }

            public long GetSequence()
            {
                return Enabled ? _sequence.Get(1, _transaction) : _sequence.Get(1);
            }

            public void Put(DatabaseEntry key, DatabaseEntry value)
            {
                if (Enabled)
                    _database.Put(key, value, _transaction);
                else
                    _database.Put(key, value);
            }

            public void Commit()
            {
                if (Enabled)
                    _transaction.Commit();
            }

            public void Discard()
            {
                if (Enabled)
                    _transaction.Discard();
            }
        }

        #endregion
    }
}
