using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DatabaseQueue
{
    public class BinarySerializer<T> : ISerializer<T>
    {
        private readonly IFormatter _formatter = new BinaryFormatter();

        #region ISerializer<T> Members

        public bool TrySerialize(T target, out object serialized)
        {
            serialized = default(T);

            try
            {
                using (var stream = new MemoryStream())
                {
                    _formatter.Serialize(stream, target);

                    var bytes = new byte[stream.Length];
                    stream.Position = 0;
                    stream.Read(bytes, 0, bytes.Length);

                    serialized = bytes;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool TryDeserialize(object value, out T deserialized)
        {
            deserialized = default(T);

            try
            {
                using (var stream = new MemoryStream((byte[]) value))
                {
                    stream.Position = 0;

                    deserialized = (T)_formatter.Deserialize(stream);
                }

                return deserialized != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}
