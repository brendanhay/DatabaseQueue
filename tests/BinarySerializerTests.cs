using DatabaseQueue.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class BinarySerializerTests : SerializerTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestInitialize(new BinarySerializer<Entity>());
        }

        [TestMethod]
        public void BinarySerializer_TrySerialize_DummyEntity_IsTrue()
        {
            Assert_TrySerialize_IsTrue(Entity);
        }

        [TestMethod]
        public void BinarySerializer_TrySerialize_TryDeserialize_DummyEntity_AreEqual()
        {
            Assert_Serialize_Then_Deserialize_AreEqual(Entity);
        }
    }
}
