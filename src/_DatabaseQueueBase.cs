﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace DatabaseQueue
{
    public abstract class DatabaseQueueBase<T> : IQueue<T>, IDisposable
    {
        private readonly ISerializer<T> _serializer;

        private int _disposed;
        private int _count;

        protected DatabaseQueueBase(IStorageSchema schema, ISerializer<T> serializer)
        {
            Schema = schema;
            _serializer = serializer;
        }
        
        protected IDbConnection Connection { get; private set; }

        protected IStorageSchema Schema { get; private set; }

        private void EnsureConnectionIsOpen()
        {
            if (Connection == null)
                throw new NullReferenceException("Ensure a call to Initialize/0 is made before using the queue");

            switch (Connection.State)
            {
                case ConnectionState.Closed:
                case ConnectionState.Broken:
                    Connection.Open();
                    break;
            }
        }

        #region Abstract / Virtual Members

        public virtual void Initialize()
        {
            Connection = CreateConnection();

            EnsureConnectionIsOpen();
            EnsureTableExists();

            _count = ExecuteCountCommand();
        }

        protected abstract IDbConnection CreateConnection();

        protected abstract IDbCommand CreateDeleteCommand(IEnumerable<object> keys);
        protected abstract IDbCommand CreateInsertCommand(out IDbDataParameter parameter);
        protected abstract IDbCommand CreateSelectCommand(int max);
        protected abstract IDbCommand CreateCountCommand();

        protected abstract void EnsureTableExists();

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
            IDbDataParameter parameter;

            using (var command = CreateInsertCommand(out parameter))
            {
                foreach (var item in items)
                {
                    object serialized;

                    if (_serializer.TrySerialize(item, out serialized))
                        parameter.Value = serialized;

                    if (command.ExecuteNonQuery() != 1) 
                        continue;
                    
                    rows++;

                    Interlocked.Increment(ref _count);
                }
            }

            return rows;
        }

        private void ExecuteSelectCommand(int max, ICollection<T> items,  
            ICollection<object> deletions)
        {
            using (var select = CreateSelectCommand(max))
            {
                using (var reader = select.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        object key = reader.GetValue(Schema.Key),
                            value = reader.GetValue(Schema.Value);

                        T item;

                        if (!_serializer.TryDeserialize(value, out item))
                            continue;

                        // Mark key for deletion
                        deletions.Add(key);

                        // Add item to the out collection
                        items.Add(item);
                    }
                }
            }
        }

        private void ExecuteDeleteCommand(ICollection<object> deletions)
        {
            if (deletions.Count <= 0) return;

            using (var delete = CreateDeleteCommand(deletions))
            {
                var rows = delete.ExecuteNonQuery();

                if (rows > 0)
                    Interlocked.Add(ref _count, -rows);
            }
        }

        private bool TryInsertMultiple(ICollection<T> items)
        {
            if (items.IsNullOrEmpty())
                return false;

            EnsureConnectionIsOpen();

            var rows = 0;

            using (var transaction = Connection.BeginTransaction())
            {
                try
                {
                    rows = ExecuteInsertCommand(items);

                    transaction.Commit();
                }
                catch (InvalidOperationException)
                {
                    transaction.Rollback();
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

            using (var transaction = Connection.BeginTransaction())
            {
                try
                {
                    var deletions = new List<object>();

                    ExecuteSelectCommand(max, items, deletions);
                    ExecuteDeleteCommand(deletions);

                    transaction.Commit();

                    success = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }

            return success && items.Count > 0;
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
                Connection.Close();
            }

            // Dispose unmanaged resources
        }

        ~DatabaseQueueBase()
        {
            Dispose(false);
        }

        #endregion

        #region IQueue<T> Members

        public int Count { get { return _count; } }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            return TryInsertMultiple(items);
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            return TrySelectAndDeleteMultiple(out items, max);
        }

        #endregion
    }
}