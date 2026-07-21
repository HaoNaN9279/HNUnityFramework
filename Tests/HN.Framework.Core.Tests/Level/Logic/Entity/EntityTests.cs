#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Entity;
using HN.Framework.Core.Capability.Event;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

using EntityType = HN.Framework.Core.Level.Logic.Entity.Entity;

namespace HN.Framework.Core.Tests.Level.Logic.Entity
{
    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void Entity_AfterSpawn_HasCorrectIdAndDefId()
        {
            var bus = new EventBus();
            var manager = new EntityManager(bus);

            var entity = manager.Spawn(100);

            Assert.That(entity.EntityId, Is.EqualTo(1u));
            Assert.That(entity.EntityDefId, Is.EqualTo(100));
        }

        [Test]
        public void Entity_Clear_ResetsFieldsToDefault()
        {
            var bus = new EventBus();
            var manager = new EntityManager(bus);
            var entity = manager.Spawn(42);

            entity.Clear();

            Assert.That(entity.EntityId, Is.EqualTo(0u));
            Assert.That(entity.EntityDefId, Is.EqualTo(0));
        }

        [Test]
        public void Entity_DespawningReusesFromReferencePool()
        {
            var bus = new EventBus();
            var manager = new EntityManager(bus);
            var entity = manager.Spawn(10);
            var id = entity.EntityId;

            // Despawn returns entity to ReferencePool
            manager.Despawn(id);

            // Spawn again - should reuse pooled instance (EntityId will be new, but EntityDefId matches the spawn parameter)
            var entity2 = manager.Spawn(10);
            Assert.That(entity2.EntityDefId, Is.EqualTo(10), "Re-spawned entity should have correct def ID");
            Assert.That(entity2.EntityId, Is.EqualTo(2u), "ID should increment: despawn doesn't affect ID counter");
            Assert.That(manager.EntityCount, Is.EqualTo(1), "Only one entity should be alive");
        }

        [Test]
        public void OwnerClientId_Default_IsNegativeOne()
        {
            var entity = ReferencePool.Acquire<EntityType>();
            entity.Initialize(1, 100);
            Assert.That(entity.OwnerClientId, Is.EqualTo(-1));
            Assert.That(entity.IsOwned, Is.False);
            ReferencePool.Release(entity);
        }

        [Test]
        public void OwnerClientId_AfterSpawnWithOwner_HasCorrectOwner()
        {
            var entity = ReferencePool.Acquire<EntityType>();
            entity.Initialize(1, 100, 3);
            Assert.That(entity.OwnerClientId, Is.EqualTo(3));
            Assert.That(entity.IsOwned, Is.True);
            ReferencePool.Release(entity);
        }

        [Test]
        public void Clear_ResetsOwnerClientId()
        {
            var entity = ReferencePool.Acquire<EntityType>();
            entity.Initialize(1, 100, 3);
            entity.Clear();
            Assert.That(entity.OwnerClientId, Is.EqualTo(-1));
            Assert.That(entity.IsOwned, Is.False);
            ReferencePool.Release(entity);
        }
    }
}
