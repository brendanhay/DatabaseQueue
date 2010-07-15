using System;

namespace DatabaseQueue.Serialization
{
    public abstract class SerializerBase<T1, T2> : ISerializer<T1, T2> where T2 : class
    {
        #region ISerializer<T> Members

        bool ISerializer<T1>.TrySerialize(T1 item, out object serialized)
        {
            T2 generic;
            var success = TrySerialize(item, out generic);

            serialized = generic;

            return success;
        }

        bool ISerializer<T1>.TryDeserialize(object serialized, out T1 item)
        {
            item = default(T1);
            var cast = serialized as T2;

            return (cast != null) && TryDeserialize(cast, out item);
        }

        ISerializer<T1, T3> ISerializer<T1>.Composite<TIntermediate, T3>(ISerializer<TIntermediate, T3> serializer)
        {
            var type = typeof(T2);

            if (!typeof(TIntermediate).IsAssignableFrom(type))
            {
                throw new InvalidCastException(string.Format("Type TIntermediate from ISerializer<T1>.Composite<TIntermediate, T3> must be assignable to {0}",
                    type));
            }

            return Composite((ISerializer<T2, T3>)serializer);
        }

        #endregion

        #region ISerializer<T1,T2> Members

        public abstract bool TrySerialize(T1 item, out T2 serialized);

        public abstract bool TryDeserialize(T2 serialized, out T1 item);

        public ISerializer<T1, T3> Composite<T3>(ISerializer<T2, T3> serializer)
            where T3 : class
        {
            return new CompositeSerializer<T1, T2, T3>(this, serializer);
        }

        #endregion
    }
}
