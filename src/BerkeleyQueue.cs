using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BerkeleyDB;

namespace DatabaseQueue
{
    public sealed class BerkeleyQueue<T> : IDatabaseQueue<T>
    {
        private readonly ISerializer<T> _serializer;

        private DatabaseEnvironment _environment;
        private Database _database;
        private Sequence _sequence;

        public BerkeleyQueue(string path, ISerializer<T> serializer)
        //: base(schema, serializer)
        {
            if (!path.EndsWith(".db"))
                throw new ArgumentException("File path must be an .db file", "path");

            _serializer = serializer;
            Path = path;
        }

        public string Path { get; private set; }

        private static void SetEntry(DatabaseEntry entry, string str)
        {
            entry.Data = Encoding.ASCII.GetBytes(str);
        }

        private static string GetEntry(DatabaseEntry entry)
        {
            var decode = new ASCIIEncoding();

            return decode.GetString(entry.Data);
        }

        #region IDatabaseQueue<T> Members

        public void Initialize()
        {
            var environmentConfig = new DatabaseEnvironmentConfig
            {
                Create = true,
                UseLogging = false,
                UseMPool = false,
                UseTxns = true
            };

            _environment = DatabaseEnvironment.Open(Environment.CurrentDirectory,
                environmentConfig);

            var databaseConfig = new BTreeDatabaseConfig
            {
                AutoCommit = true,
                Creation = CreatePolicy.IF_NEEDED,
                Env = _environment
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

            SetEntry(sequenceConfig.key, "ex_csharp_sequence");
            sequenceConfig.SetRange(Int64.MinValue, Int64.MaxValue);

            _sequence = new Sequence(sequenceConfig);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _sequence.Close();
            _database.Close();
            _environment.Close();
        }

        #endregion

        #region IQueue<T> Members

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            var transaction = _environment.BeginTransaction();

            try
            {
                var key = new DatabaseEntry();
                var value = new DatabaseEntry();

                foreach (var item in items)
                {
                    object serialized;

                    if (!_serializer.TrySerialize(item, out serialized))
                        continue;

                    SetEntry(key, _sequence.Get(1).ToString());
                    SetEntry(value, serialized.ToString());

                    _database.Put(key, value);
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
                    
                    if (_serializer.TryDeserialize(GetEntry(cursor.Current.Value), out deserialized))
                        items.Add(deserialized);
                }
            }

            return items.Count > 0;
        }

        #endregion
    }
}
