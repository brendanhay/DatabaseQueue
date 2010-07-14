namespace DatabaseQueue.Serialization
{
    public interface ISerializer<T>
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
