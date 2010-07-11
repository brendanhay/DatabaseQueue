using System.Collections.Generic;

namespace DatabaseQueue
{
    public interface IQueue<T>
    {
        bool TryEnqueueMultiple(ICollection<T> items, int timeout);

        bool TryDequeueMultiple(out ICollection<T> items, int max, int timeout);
    }
}
