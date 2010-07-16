using System;
using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    /// <summary>
    /// Base interface for all queues in the Collections namespace.
    /// </summary>
    /// <typeparam name="T">The item type to be stored in the queue.</typeparam>
    public interface IQueue<T> : IDisposable
    {
        /// <summary>
        /// Number of items currently in the queue.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// If the queue is synchronized (thread-safe).
        /// </summary>
        bool Synchronized { get; }

        /// <summary>
        /// The object to be used in lock operations.
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// Enqueue a collection of items into the queue.
        /// </summary>
        /// <param name="items">Items to enqueue.</param>
        /// <returns>True if successful.</returns>
        bool TryEnqueueMultiple(ICollection<T> items);

        /// <summary>
        /// Dequeue a collection of items up to a specified maximum.
        /// </summary>
        /// <param name="items">Collection of items passed by reference.</param>
        /// <param name="max">Maximum number of items to return.</param>
        /// <returns>True if successful.</returns>
        bool TryDequeueMultiple(out ICollection<T> items, int max);
    }
}
