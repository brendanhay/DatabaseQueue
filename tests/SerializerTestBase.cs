using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    public abstract class SerializerTestBase
    {
        protected void TestInitialize(ISerializer<DummyEntity> serializer)
        {
            Serializer = serializer;
            Entity = DummyEntity.Create();
        }

        protected ISerializer<DummyEntity> Serializer { get; private set; }

        protected DummyEntity Entity { get; private set; }

        #region Assertions

        protected object AssertTrySerializeIsTrue(DummyEntity entity)
        {
            object serialized;

            Assert.IsTrue(Serializer.TrySerialize(entity, out serialized));
            Assert.IsNotNull(serialized);

            return serialized;
        }

        protected DummyEntity AssertTryDeserializeIsTrue(object serialized)
        {
            DummyEntity deserialized;

            Assert.IsTrue(Serializer.TryDeserialize(serialized, out deserialized));
            Assert.IsNotNull(deserialized);

            return deserialized;
        }

        protected void AssertSerializeThenDeserializeAreEqual(DummyEntity entity)
        {
            var serialized = AssertTrySerializeIsTrue(entity);
            var deserialized = AssertTryDeserializeIsTrue(serialized);

            Assert.AreEqual(entity, deserialized);
        }

        #endregion
    }
}
