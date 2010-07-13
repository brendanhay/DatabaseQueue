using System;

namespace DatabaseQueue.Collections
{
    public interface IDatabaseQueue<T> : IQueue<T>, IDisposable
    {
        void Initialize();
    }
}
