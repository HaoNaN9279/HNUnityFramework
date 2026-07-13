#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Unity.Capability.Physics;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Physics
{
    [TestFixture]
    public class UnityBodyTests
    {
        private GameObject _go = null!;
        private UnityBody _body = null!;
        private Rigidbody _rb = null!;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestBody");
            _go.hideFlags = HideFlags.HideAndDontSave;
            _rb = _go.AddComponent<Rigidbody>();
            _body = _go.AddComponent<UnityBody>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }
        }

        [Test]
        public void Position_SetAndGet_Roundtrips()
        {
            var pos = new PhysicsVector3(10f, 20f, 30f);
            _body.Position = pos;
            var result = _body.Position;
            Assert.AreEqual(pos, result);
        }

        [Test]
        public void Velocity_SetAndGet_Roundtrips()
        {
            var vel = new PhysicsVector3(5f, 0f, 0f);
            _body.Velocity = vel;
            var result = _body.Velocity;
            Assert.AreEqual(vel.X, result.X, 1e-6f);
            Assert.AreEqual(vel.Y, result.Y, 1e-6f);
            Assert.AreEqual(vel.Z, result.Z, 1e-6f);
        }

        [Test]
        public void Mass_SetAndGet_Roundtrips()
        {
            _body.Mass = 10f;
            Assert.AreEqual(10f, _body.Mass);
        }

        [Test]
        public void IsKinematic_SetAndGet_Roundtrips()
        {
            _body.IsKinematic = true;
            Assert.IsTrue(_body.IsKinematic);
            _body.IsKinematic = false;
            Assert.IsFalse(_body.IsKinematic);
        }

        [Test]
        public void BodyType_Dynamic_Default()
        {
            Assert.AreEqual(BodyType.Dynamic, _body.BodyType);
        }

        [Test]
        public void InstanceId_ReturnsNonZero()
        {
            Assert.That(_body.InstanceId, Is.Not.EqualTo(0));
        }

        [Test]
        public void AddForce_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _body.AddForce(new PhysicsVector3(0f, 10f, 0f)));
        }

        [Test]
        public void AddTorque_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _body.AddTorque(new PhysicsVector3(0f, 1f, 0f)));
        }
    }
}
