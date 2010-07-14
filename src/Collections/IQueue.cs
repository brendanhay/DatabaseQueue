using System;
using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    public interface IQueue<T> : IDisposable
    {
        int Count { get; }

        bool Synchronized { get; }

        object SyncRoot { get; }

        bool TryEnqueueMultiple(ICollection<T> items);
        
        bool TryDequeueMultiple(out ICollection<T> items, int max);
    }
}
