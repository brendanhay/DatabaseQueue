using System;
using System.IO;
using System.Xml.Serialization;

namespace DatabaseQueue.Serialization
{
    public class XmlSerializer<T> : SerializerBase<T, string>
    {
        private readonly XmlSerializer _serializer;

        public XmlSerializer() : this(new XmlSerializer(typeof(T))) { }

        public XmlSerializer(XmlSerializer serializer)
        {
            _serializer = serializer;
        }

        public override bool TrySerialize(T item, out string serialized)
        {
            serialized = default(string);

            try
            {
                using (var stream = new MemoryStream())
                {
                    _serializer.Serialize(stream, item);

                    stream.Position = 0;

                    using (var reader = new StreamReader(stream))
                        serialized = reader.ReadToEnd();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool TryDeserialize(string serialized, out T item)
        {
            item = default(T);

            try
            {
                using (var reader = new StringReader(serialized))
                    item = (T)_serializer.Deserialize(reader);

                return item != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
