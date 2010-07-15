namespace DatabaseQueue.Serialization
{
    public interface ISerializer
    {
        bool TrySerialize(object item, out object serialized);

        bool TryDeserialize(object serialized, out object item); 
    }

    public interface ISerializer<T> : ISerializer
    {
        bool TrySerialize(T item, out object serialized);

        bool TryDeserialize(object serialized, out T item);        
    }

    public interface ISerializer<T, TSerialized> : ISerializer<T>
    {
        bool TrySerialize(T item, out TSerialized serialized);

        bool TryDeserialize(TSerialized serialized, out T item);
    }
}
