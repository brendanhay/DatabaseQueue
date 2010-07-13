using DatabaseQueue.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class JsonSerializerTests : SerializerTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestInitialize(new JsonSerializer<Entity>());
        }

        [TestMethod]
        public void JsonSerializer_TrySerialize_DummyEntity_IsTrue()
        {
            AssertTrySerializeIsTrue(Entity);
        }

        [TestMethod]
        public void JsonSerializer_TrySerialize_TryDeserialize_DummyEntity_AreEqual()
        {
            AssertSerializeThenDeserializeAreEqual(Entity);
        }
    }
}
