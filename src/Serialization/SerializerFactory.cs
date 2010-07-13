using DatabaseQueue.Data;

namespace DatabaseQueue.Serialization
{
    public interface ISerializerFactory<T>
    {
        ISerializer<T> Create(FormatType format);
    }

    public class SerializerFactory<T> : ISerializerFactory<T>
    {
        #region ISerializerFactory<T> Members

        public ISerializer<T> Create(FormatType format)
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

        #endregion
    }
}
