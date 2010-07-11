using System;
using Newtonsoft.Json;

namespace DatabaseQueue
{
    public class JsonSerializer<T> : ISerializer<T>
    {
        #region ISerializer<T> Members

        public bool TrySerialize(T target, out object serialized)
        {
            serialized = default(T);

            try
            {
                serialized = JsonConvert.SerializeObject(target);

                return serialized != null;
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
                deserialized = JsonConvert.DeserializeObject<T>(value.ToString());

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
