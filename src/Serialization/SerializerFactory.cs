using DatabaseQueue.Data;

namespace DatabaseQueue.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer<T> Create<T>(FormatType format);

        ISerializer<T, byte[]> CreateBinaryComposite<T>(FormatType format);
    }

    /// <summary>
    /// A factory for creating serializers.
    /// </summary>
    public class SerializerFactory : ISerializerFactory
    {
        #region ISerializerFactory Members

        public ISerializer<T> Create<T>(FormatType format)
        {
            ISerializer<T> serializer;

            switch (format)
            {
                case FormatType.Xml:
                    serializer = new XmlSerializer<T>();
                    break;
                case FormatType.Json:
                    serializer = new JsonSerializer<T>();
                    break;
                default:
                    serializer = new BinarySerializer<T>();
                    break;
            }

            return serializer;
        }

        public ISerializer<T, byte[]> CreateBinaryComposite<T>(FormatType intermediate)
        {
            ISerializer<T, byte[]> serializer;

            switch (intermediate)
            {
                case FormatType.Binary:
                    serializer = new BinarySerializer<T>();
                    break;
                default:
                    serializer = Create<T>(intermediate).PostSerializeWith(new BinarySerializer<string>());
                    break;
            }

            return serializer;
        }

        #endregion
    }
}
