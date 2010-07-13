using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseQueue.Collections
{
    public class BufferedQueue<T> : IQueue<T>
    {
        private readonly IQueue<T> _overflow, _buffer;
        private readonly int _bufferMax, _bufferMin;

        public BufferedQueue(IDatabaseQueue<T> overflow, IQueue<T> buffer, int bufferMax, 
            int bufferMin)
        {
            overflow.Initialize();

            _overflow = overflow;
            _buffer = buffer;
            _bufferMax = bufferMax;
            _bufferMin = bufferMin;
        }

        #region IQueue<T> Members

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool TryEnqueueMultiple(ICollection<T> items)
        {
            throw new NotImplementedException();
        }

        public bool TryDequeueMultiple(out ICollection<T> items, int max)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
