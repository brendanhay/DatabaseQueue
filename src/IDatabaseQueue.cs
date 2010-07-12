using System;

namespace DatabaseQueue
{
    public interface IDatabaseQueue<T> : IQueue<T>, IDisposable
    {
        void Initialize();
    }
}
