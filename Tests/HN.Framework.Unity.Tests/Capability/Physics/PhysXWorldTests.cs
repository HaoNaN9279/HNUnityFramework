#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Unity.Capability.Physics;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Physics
{
    [TestFixture]
    public class PhysXWorldTests
    {
        private PhysXWorld _world = null!;

        [SetUp]
        public void SetUp()
        {
            _world = new PhysXWorld(PhysicsDimension.D3);
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
        }

        [Test]
        public void Dimension_ReturnsD3()
        {
            Assert.AreEqual(PhysicsDimension.D3, _world.Dimension);
        }

        [Test]
        public void CreateBody_ReturnsNonNullBody()
        {
            var shape = new BoxShape();
            var body = _world.CreateBody(shape, PhysicsVector3.Zero, PhysicsQuaternion.Identity, BodyType.Dynamic, "Default");
            Assert.IsNotNull(body);
        }

        [Test]
        public void CreateAndDestroyBody_Lifecycle()
        {
            var shape = new BoxShape();
            var body = _world.CreateBody(shape, PhysicsVector3.Zero, PhysicsQuaternion.Identity, BodyType.Dynamic, "Default");
            Assert.IsNotNull(body);

            _world.DestroyBody(body);
        }

        [Test]
        public void CreateBody_SetsPosition()
        {
            var shape = new BoxShape();
            var pos = new PhysicsVector3(10f, 20f, 30f);
            var body = _world.CreateBody(shape, pos, PhysicsQuaternion.Identity, BodyType.Dynamic, "Default");
            var result = body.Position;
            Assert.AreEqual(pos.X, result.X, 1e-6f);
            Assert.AreEqual(pos.Y, result.Y, 1e-6f);
            Assert.AreEqual(pos.Z, result.Z, 1e-6f);
        }

        [Test]
        public void CreateBody_WithSphereShape_CreatesSphereCollider()
        {
            var shape = new SphereShape(1f);
            var body = _world.CreateBody(shape, PhysicsVector3.Zero, PhysicsQuaternion.Identity, BodyType.Dynamic, "Default");
            Assert.IsNotNull(body);
        }

        [Test]
        public void CreateBody_WithCapsuleShape_CreatesCapsuleCollider()
        {
            var shape = new CapsuleShape(0.5f, 2f);
            var body = _world.CreateBody(shape, PhysicsVector3.Zero, PhysicsQuaternion.Identity, BodyType.Dynamic, "Default");
            Assert.IsNotNull(body);
        }

        [Test]
        public void Raycast_NoObstacle_ReturnsFalse()
        {
            bool hit = _world.Raycast(
                new PhysicsVector3(0f, 0f, 0f),
                new PhysicsVector3(0f, 0f, 1f),
                out var hitInfo, 100f, "Default");
            Assert.IsFalse(hit);
        }

        [Test]
        public void Step_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _world.Step(0.016f));
        }

        [Test]
        public void Dispose_MultipleTimes_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _world.Dispose();
                _world.Dispose();
            });
        }
    }
}
