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

        protected object AssertTrySerializeIsTrue(Entity entity)
        {
            object serialized;

            Assert.IsTrue(Serializer.TrySerialize(entity, out serialized));
            Assert.IsNotNull(serialized);

            return serialized;
        }

        protected Entity AssertTryDeserializeIsTrue(object serialized)
        {
            Entity deserialized;

            Assert.IsTrue(Serializer.TryDeserialize(serialized, out deserialized));
            Assert.IsNotNull(deserialized);

            return deserialized;
        }

        protected void AssertSerializeThenDeserializeAreEqual(Entity entity)
        {
            var serialized = AssertTrySerializeIsTrue(entity);
            var deserialized = AssertTryDeserializeIsTrue(serialized);

            Assert.AreEqual(entity, deserialized);
        }

        #endregion
    }
}
