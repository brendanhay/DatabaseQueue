namespace DatabaseQueue
{
    public interface ISerializer<T>
    {
        bool TrySerialize(T target, out object serialized);

        bool TryDeserialize(object value, out T deserialized);
    }
}
