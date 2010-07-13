using System.Collections.Generic;

namespace DatabaseQueue.Collections
{
    public interface IQueue<T>
    {
        int Count { get; }

        bool TryEnqueueMultiple(ICollection<T> items);

        bool TryDequeueMultiple(out ICollection<T> items, int max);
    }
}
