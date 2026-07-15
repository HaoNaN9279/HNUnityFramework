#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Entity;
using HN.Framework.Core.Capability.Serialization;
using HN.Framework.Core.Driver.Common.Serialization;

namespace HN.Framework.Core.Tests.Level.Logic.Entity
{
    [TestFixture]
    public class EntityFormattersTests
    {
        [SetUp]
        public void SetUp()
        {
            FormattersInitializer.RegisterAll();
        }

        [Test]
        public void Serialize_Deserialize_Roundtrip()
        {
            var original = new Entity();
            original.Initialize(100, 200, 300);

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Entity>(data);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.EntityId, Is.EqualTo(100u));
            Assert.That(result.EntityDefId, Is.EqualTo(200));
            Assert.That(result.OwnerClientId, Is.EqualTo(300));
        }

        [Test]
        public void Serialize_Deserialize_DefaultOwnerClientId()
        {
            var original = new Entity();
            original.Initialize(1, 2);

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Entity>(data);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.EntityId, Is.EqualTo(1u));
            Assert.That(result.EntityDefId, Is.EqualTo(2));
            Assert.That(result.OwnerClientId, Is.EqualTo(-1));
        }

        [Test]
        public void Serialize_Deserialize_BoundaryValues()
        {
            var original = new Entity();
            original.Initialize(uint.MaxValue, int.MaxValue, 0);

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Entity>(data);

            Assert.That(result.EntityId, Is.EqualTo(uint.MaxValue));
            Assert.That(result.EntityDefId, Is.EqualTo(int.MaxValue));
            Assert.That(result.OwnerClientId, Is.EqualTo(0));
        }

        [Test]
        public void Serialize_Null_ReturnsNull()
        {
            Entity nullEntity = null;

            byte[] data = MemoryPackSerializer.Serialize(nullEntity);
            var result = MemoryPackSerializer.Deserialize<Entity>(data);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void IsOwned_AfterDeserialization_Correct()
        {
            var original = new Entity();
            original.Initialize(1, 2, 5);

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Entity>(data);

            Assert.That(result.IsOwned, Is.True);
            Assert.That(result.EntityId, Is.EqualTo(1u));
        }

        [Test]
        public void IsOwned_AfterDeserialization_NegativeOwner_NotOwned()
        {
            var original = new Entity();
            original.Initialize(1, 2, -1);

            byte[] data = MemoryPackSerializer.Serialize(original);
            var result = MemoryPackSerializer.Deserialize<Entity>(data);

            Assert.That(result.IsOwned, Is.False);
        }
    }
}
