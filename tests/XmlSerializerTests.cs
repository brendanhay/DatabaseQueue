﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests
{
    [TestClass]
    public class XmlSerializerTests : SerializerTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestInitialize(new XmlSerializer<Entity>());
        }

        [TestMethod]
        public void XmlSerializer_TrySerialize_DummyEntity_IsTrue()
        {
            AssertTrySerializeIsTrue(Entity);
        }

        [TestMethod]
        public void XmlSerializer_TrySerialize_TryDeserialize_DummyEntity_AreEqual()
        {
            AssertSerializeThenDeserializeAreEqual(Entity);
        }
    }
}
