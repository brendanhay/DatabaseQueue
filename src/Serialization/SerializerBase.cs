namespace DatabaseQueue.Serialization
{
    public abstract class SerializerBase<T, TSerialized> : ISerializer<T, TSerialized> 
        where TSerialized : class
    {
        private readonly ISerializer<T> _inner;

        protected SerializerBase(ISerializer<T> inner)
        {
            _inner = inner;
        }

        protected abstract bool TrySerialize(T item, out TSerialized serialized);
       
        protected abstract bool TryDeserialize(TSerialized serialized, out T item);

        #region ISerializer Members

        // TODO: Random ideas

        bool ISerializer.TrySerialize(object item, out object serialized)
        {
            var preserialized;

            _inner.TrySerialize(item, out preserialized) && ISerializer.TrySerializer(preserialized, out serialized);
        }

        bool ISerializer.TryDeserialize(object serialized, out object item)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region ISerializer<T> Members

        bool ISerializer<T>.TrySerialize(T item, out object serialized)
        {
            TSerialized generic;
            var success = TrySerialize(item, out generic);

            serialized = generic;

            return success;
        }

        bool ISerializer<T>.TryDeserialize(object serialized, out T item)
        {
            item = default(T);
            var cast = serialized as TSerialized;

            return (cast != null) && TryDeserialize(cast, out item);
        }

        #endregion

        #region ISerializer<T,TSerialized> Members

        bool ISerializer<T, TSerialized>.TrySerialize(T item, out TSerialized serialized)
        {
            
        }

        bool ISerializer<T, TSerialized>.TryDeserialize(TSerialized serialized, out T item)
        {

        }

        #endregion
    }
}
