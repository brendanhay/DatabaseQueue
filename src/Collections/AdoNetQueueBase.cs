using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using DatabaseQueue.Data;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Extensions;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// An ADO.NET base class implementation of IQueue<typeparam name="T"/>
    /// Non-blocking / non-sychronized by default.
    /// Implements: <see cref="IQueue{T}" />
    /// </summary>
    /// <typeparam name="T">The item type the queue/serializer will support.</typeparam>
    internal abstract class AdoNetQueueBase<T> : IQueue<T>
    {
        private readonly IDbConnection _connection;
        private readonly ISerializer<T> _serializer;
        private readonly IQueuePerformanceCounter _performance;

        private int _disposed, _count;

        /// <summary>
        /// Base class for queues which store items in an ADO.NET Database.
        /// </summary>
        /// <param name="connection">Open or closed connection which will be used for all commands.</param>
        /// <param name="schema">Schema defining the table, names and Db/Sql types for parameters.</param>
        /// <param name="serializer">Serializer used to serialize/deserialize objects into Db types.</param>
        /// <param name="checkTableExists">
        /// If true, a seperate round trip to the database using GetTableExistsCommandText/0 
        /// is made before deciding to call GetCreateTableCommandText based on the result.
        /// </param>
        /// <param name="performance">
        /// The performance counter to measure item throughput, 
        /// null if performance measurements won't be used.
        /// </param>
        protected AdoNetQueueBase(IDbConnection connection, IStorageSchema schema,
            ISerializer<T> serializer, bool checkTableExists, IQueuePerformanceCounter performance)
        {
            _connection = connection;
            _serializer = serializer;
            _performance = performance;
            Schema = schema;

            EnsureConnectionIsOpen();
            EnsureTableExists(checkTableExists);
            _count = ExecuteCountCommand();
        }

        protected IStorageSchema Schema { get; private set; }

        private void EnsureConnectionIsOpen()
        {
            if (_connection == null)
                throw new NullReferenceException("Ensure a call to Initialize/0 is made before using the queue");

            switch (_connection.State)
            {
                case ConnectionState.Closed:
                case ConnectionState.Broken:
                    _connection.Open();
                    break;
            }
        }

        private void EnsureTableExists(bool checkTableExists)
        {
            if (checkTableExists)
            {
                using (var exists = CreateCommand(Schema.TableExistsCommandText))
                {
                    if ((int)exists.ExecuteScalar() > 0)
                        return;
                }
            }

            var createText = Schema.CreateTableCommandText;

            using (var create = CreateCommand(createText))
                create.ExecuteNonQuery();
        }

        #region Command Creation

        private IDbCommand CreateInsertCommand(out IDbDataParameter valueParameter)
        {
            var commandText = Schema.InsertCommandText;
            var command = CreateCommand(commandText);

            valueParameter = command.CreateParameter();
            valueParameter.DbType = Schema.Value.ParameterType;
            command.Parameters.Add(valueParameter);

            return command;
        }

        private IDbCommand CreateSelectCommand(int max)
        {
            var commandText = Schema.GetSelectCommandText(max);

            return CreateCommand(commandText);
        }

        private IDbCommand CreateDeleteCommand(out IDbDataParameter keyParameter)
        {
            var commandText = Schema.DeleteCommandText;
            var command = CreateCommand(commandText);

            keyParameter = command.CreateParameter();
            keyParameter.DbType = Schema.Key.ParameterType;
            command.Parameters.Add(keyParameter);

            return command;
        }

        private IDbCommand CreateCountCommand()
        {
            var commandText = Schema.CountCommandText;

            return CreateCommand(commandText);
        }

        private IDbCommand CreateCommand(string commandText)
        {
            var command = _connection.CreateCommand();
            command.CommandText = commandText;

            return command;
        }

        #endregion

        #region Bulk Insert / Select / Delete

        private int ExecuteCountCommand()
        {
            using (var command = CreateCountCommand())
                return Convert.ToInt32(command.ExecuteScalar());
        }

        private int ExecuteInsertCommand(IEnumerable<T> items)
        {
            var rows = 0;
            IDbDataParameter insertParameter;

            using (var command = CreateInsertCommand(out insertParameter))
            {
                foreach (var item in items)
                {
                    var start = DateTime.Now;

                    object serialized;

                    if (_serializer.TrySerialize(item, out serialized))
                        insertParameter.Value = serialized;

                    if (command.ExecuteNonQuery() != 1)
                        continue;

                    rows++;

                    Interlocked.Increment(ref _count);

                    if (_performance != null)
                        _performance.Enqueue(true, start, 0);
                }
            }

            return rows;
        }

        private void ExecuteSelectAndDeleteCommand(int max, ICollection<T> items)
        {
            using (var select = CreateSelectCommand(max))
            {
                IDbDataParameter deleteParameter;

                using (var delete = CreateDeleteCommand(out deleteParameter))
                {
                    using (var reader = select.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var start = DateTime.Now;

                            var value = reader.GetValue(Schema.Value);

                            deleteParameter.Value = reader.GetValue(Schema.Key);
                            T item;

                            if (!_serializer.TryDeserialize(value, out item)
                                || delete.ExecuteNonQuery() != 1)
                            {
                                continue;
                            }

                            items.Add(item);

                            Interlocked.Decrement(ref _count);

                            if (_performance != null)
                                _performance.Dequeue(true, start, 0);
                        }
                    }
                }
            }
        }

        private bool TryInsertMultiple(ICollection<T> items)
        {
            if (items.IsNullOrEmpty())
                return false;

            EnsureConnectionIsOpen();

            var rows = 0;

            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    rows = ExecuteInsertCommand(items);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    return false;
                }
            }

            return rows == items.Count;
        }

        private bool TrySelectAndDeleteMultiple(out ICollection<T> items, int max)
        {
            var success = false;
            items = new List<T>();

            if (max < 1)
                return false;

            EnsureConnectionIsOpen();

            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    ExecuteSelectAndDeleteCommand(max, items);

                    transaction.Commit();

                    success = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    success = false;
                }
            }

            return success && items.Count > 0;
        }

        #endregion

        #region IQueue<T> Members

        public int Count { get { return _count; } }

        public bool Synchronized { get { return false; } }

        public object SyncRoot { get { return this; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            return TryInsertMultiple(items);
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            return TrySelectAndDeleteMultiple(out items, max);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing)
            {
                // Dispose managed resources
                _connection.Close();
            }

            // Dispose unmanaged resources
        }

        ~AdoNetQueueBase()
        {
            Dispose(false);
        }

        #endregion
    }
}
