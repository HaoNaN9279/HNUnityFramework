#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Entity;
using HN.Framework.Core.Capability.Event;

namespace HN.Framework.Core.Tests.Level.Logic.Entity
{
    [TestFixture]
    public class EntityManagerTests
    {
        [Test]
        public void Spawn_ReturnsEntityWithUniqueIncrementingId()
        {
            var manager = CreateManager();

            var entity1 = manager.Spawn(10);
            var entity2 = manager.Spawn(20);

            Assert.That(entity2.EntityId, Is.GreaterThan(entity1.EntityId));
            Assert.That(entity2.EntityId, Is.EqualTo(entity1.EntityId + 1));
        }

        [Test]
        public void Spawn_MultipleEntities_AllHaveDifferentIds()
        {
            var manager = CreateManager();
            var ids = new System.Collections.Generic.HashSet<uint>();

            for (int i = 0; i < 100; i++)
            {
                var entity = manager.Spawn(i);
                ids.Add(entity.EntityId);
            }

            Assert.That(ids.Count, Is.EqualTo(100));
        }

        [Test]
        public void Despawn_ExistingEntity_EntityCountDecreases()
        {
            var manager = CreateManager();
            manager.Spawn(10);
            manager.Spawn(20);

            Assert.That(manager.EntityCount, Is.EqualTo(2));

            manager.Despawn(1u);

            Assert.That(manager.EntityCount, Is.EqualTo(1));
        }

        [Test]
        public void Despawn_NonExistentEntity_DoesNotThrow()
        {
            var manager = CreateManager();

            Assert.DoesNotThrow(() => manager.Despawn(999u));
        }

        [Test]
        public void GetEntity_ExistingId_ReturnsCorrectEntity()
        {
            var manager = CreateManager();
            var entity = manager.Spawn(42);

            var result = manager.GetEntity(entity.EntityId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.EntityDefId, Is.EqualTo(42));
        }

        [Test]
        public void GetEntity_NonExistentId_ReturnsNull()
        {
            var manager = CreateManager();

            var result = manager.GetEntity(999u);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void EntityCount_MatchesSpawnMinusDespawn()
        {
            var manager = CreateManager();

            manager.Spawn(10);
            manager.Spawn(20);
            manager.Spawn(30);
            Assert.That(manager.EntityCount, Is.EqualTo(3));

            manager.Despawn(1u);
            Assert.That(manager.EntityCount, Is.EqualTo(2));

            manager.Despawn(2u);
            Assert.That(manager.EntityCount, Is.EqualTo(1));

            manager.Despawn(3u);
            Assert.That(manager.EntityCount, Is.EqualTo(0));
        }

        [Test]
        public void Spawn_PublishesEntitySpawnedEventViaEventBus()
        {
            var bus = new EventBus();
            var manager = new EntityManager(bus);

            uint receivedId = 0;
            int receivedDefId = 0;
            bus.Subscribe<EntitySpawnedEvent>(evt =>
            {
                receivedId = evt.EntityId;
                receivedDefId = evt.EntityDefId;
            });

            manager.Spawn(77);

            Assert.That(receivedId, Is.EqualTo(1u));
            Assert.That(receivedDefId, Is.EqualTo(77));
        }

        [Test]
        public void Despawn_PublishesEntityDespawnedEventViaEventBus()
        {
            var bus = new EventBus();
            var manager = new EntityManager(bus);
            var entity = manager.Spawn(50);

            uint receivedId = 0;
            bus.Subscribe<EntityDespawnedEvent>(evt =>
            {
                receivedId = evt.EntityId;
            });

            manager.Despawn(entity.EntityId);

            Assert.That(receivedId, Is.EqualTo(entity.EntityId));
        }

        [Test]
        public void Despawn_AfterEntity_GetEntityReturnsNull()
        {
            var manager = CreateManager();
            var entity = manager.Spawn(99);
            var id = entity.EntityId;

            manager.Despawn(id);

            Assert.That(manager.GetEntity(id), Is.Null);
            Assert.That(manager.EntityCount, Is.EqualTo(0));
        }

        private static EntityManager CreateManager()
        {
            return new EntityManager(new EventBus());
        }
    }
}
