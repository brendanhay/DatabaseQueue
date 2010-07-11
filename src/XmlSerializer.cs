using System.IO;
using System.Xml.Serialization;

namespace DatabaseQueue
{
    public class XmlSerializer<T> : ISerializer<T>
    {
        private readonly XmlSerializer _serializer;

        public XmlSerializer() : this(new XmlSerializer(typeof(T))) { }

        public XmlSerializer(XmlSerializer serializer)
        {
            _serializer = serializer;
        }

        #region ISerializer<T> Members

        public bool TrySerialize(T target, out object serialized)
        {
            serialized = default(T);

            try
            {
                using (var stream = new MemoryStream())
                {
                    _serializer.Serialize(stream, target);

                    stream.Position = 0;

                    using (var reader = new StreamReader(stream))
                        serialized = reader.ReadToEnd();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryDeserialize(object value, out T deserialized)
        {
            deserialized = default(T);

            try
            {
                using (var reader = new StringReader(value.ToString()))
                    deserialized = (T)_serializer.Deserialize(reader);

                return deserialized != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
