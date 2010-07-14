using System;
using Newtonsoft.Json;

namespace DatabaseQueue.Serialization
{
    public class JsonSerializer<T> : SerializerBase<T, string>
    {
        public override bool TrySerialize(T item, out string serialized)
        {
            serialized = default(string);

            try
            {
                serialized = JsonConvert.SerializeObject(item);

                return serialized != null;
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
                item = JsonConvert.DeserializeObject<T>(serialized);

                return item != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
