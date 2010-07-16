using DatabaseQueue.Benchmark;
using DatabaseQueue.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    public abstract class SerializerTestBase
    {
        protected void TestInitialize(ISerializer<Entity> serializer)
        {
            Serializer = serializer;
            Entity = Entity.Create();
        }

        protected ISerializer<Entity> Serializer { get; private set; }

        protected Entity Entity { get; private set; }

        #region Assertions

        protected object Assert_TrySerialize_IsTrue(Entity entity)
        {
            object serialized;

            Assert.IsTrue(Serializer.TrySerialize(entity, out serialized));
            Assert.IsNotNull(serialized);

            return serialized;
        }

        protected Entity Assert_TryDeserialize_IsTrue(object serialized)
        {
            Entity deserialized;

            Assert.IsTrue(Serializer.TryDeserialize(serialized, out deserialized));
            Assert.IsNotNull(deserialized);

            return deserialized;
        }

        protected void Assert_Serialize_Then_Deserialize_AreEqual(Entity entity)
        {
            var serialized = Assert_TrySerialize_IsTrue(entity);
            var deserialized = Assert_TryDeserialize_IsTrue(serialized);

            Assert.AreEqual(entity, deserialized);
        }

        #endregion
    }
}
