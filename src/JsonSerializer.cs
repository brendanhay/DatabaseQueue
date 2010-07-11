using System;
using Newtonsoft.Json;

namespace DatabaseQueue
{
    public class JsonSerializer<T> : ISerializer<T>
    {
        #region ISerializer<T> Members

        public bool TrySerialize(T target, out object serialized)
        {
            try
            {
                serialized = JsonConvert.SerializeObject(target);

                return true;
            }
            catch (Exception ex)
            {
                serialized = null;

                return false;
            }
        }

        public bool TryDeserialize(object value, out T deserialized)
        {
            try
            {
                deserialized = JsonConvert.DeserializeObject<T>(value.ToString());

                return true;
            }
            catch
            {
                deserialized = default(T);

                return false;
            }
        }

        #endregion
    }
}
