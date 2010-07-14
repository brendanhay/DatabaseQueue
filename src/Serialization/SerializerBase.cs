namespace DatabaseQueue.Serialization
{
    public abstract class SerializerBase<T, TSerialized> : ISerializer<T, TSerialized> 
        where TSerialized : class
    {
        #region ISerializer<T,TSerialized> Members

        public abstract bool TrySerialize(T item, out TSerialized serialized);

        public abstract bool TryDeserialize(TSerialized serialized, out T item);

        #endregion

        #region ISerializer<T> Members

        bool ISerializer<T>.TrySerialize(T item, out object serialized)
        {
            TSerialized str;
            var success = TrySerialize(item, out str);

            serialized = str;

            return success;
        }

        bool ISerializer<T>.TryDeserialize(object serialized, out T item)
        {
            item = default(T);
            var cast = serialized as TSerialized;

            return (cast != null) && TryDeserialize(cast, out item);
        }

        #endregion
    }
}
