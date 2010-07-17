namespace DatabaseQueue.Serialization
{
    /// <summary>
    /// A serializer which wraps two existing <see cref="ISerializer{T1,T2}" /> 
    /// to produce a composite serialization chain.
    /// </summary>
    /// <typeparam name="T1">Deserialized</typeparam>
    /// <typeparam name="T2">Intermediate</typeparam>
    /// <typeparam name="T3">Serialized</typeparam>
    public class CompositeSerializer<T1, T2, T3> : SerializerBase<T1, T3> where T3 : class
    {
        private readonly ISerializer<T1, T2> _pre;
        private readonly ISerializer<T2, T3> _post;

        public CompositeSerializer(ISerializer<T1, T2> pre, ISerializer<T2, T3> post)
        {
            _pre = pre;
            _post = post;
        }

        public override bool TrySerialize(T1 item, out T3 serialized)
        {
            T2 intermediary;
            serialized = default(T3);

            return _pre.TrySerialize(item, out intermediary) &&
                _post.TrySerialize(intermediary, out serialized);
        }

        public override bool TryDeserialize(T3 serialized, out T1 item)
        {
            T2 intermediary;
            item = default(T1);

            return _post.TryDeserialize(serialized, out intermediary)
                && _pre.TryDeserialize(intermediary, out item);
        }
    }
}
