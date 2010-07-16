namespace DatabaseQueue.Serialization
{
    public interface ISerializer<T> 
    {
        bool TrySerialize(T item, out object serialized);

        bool TryDeserialize(object serialized, out T item);

        ISerializer<T, T3> PostSerializeWith<T2, T3>(ISerializer<T2, T3> serializer)
            where T2 : class
            where T3 : class;
    }

    public interface ISerializer<T1, T2> : ISerializer<T1>
    {
        bool TrySerialize(T1 item, out T2 serialized);

        bool TryDeserialize(T2 serialized, out T1 item);

        ISerializer<T1, T3> PostSerializeWith<T3>(ISerializer<T2, T3> serializer)
            where T3 : class;
    }
}
