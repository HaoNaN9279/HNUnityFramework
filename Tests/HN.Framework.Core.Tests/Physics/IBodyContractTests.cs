#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Physics
{
    /// <summary>
    /// IBody 接口的模拟实现，用于合约测试。
    /// </summary>
    internal sealed class MockBody : IBody
    {
        /// <summary>
        /// 获取上次调用 AddForce 时传入的力向量
        /// </summary>
        public PhysicsVector3? LastForce { get; private set; }

        /// <summary>
        /// 获取上次调用 AddTorque 时传入的扭矩向量
        /// </summary>
        public PhysicsVector3? LastTorque { get; private set; }

        /// <summary>
        /// 获取 Dispose 是否被调用
        /// </summary>
        public bool Disposed { get; private set; }

        /// <inheritdoc/>
        public PhysicsVector3 Position { get; set; }

        /// <inheritdoc/>
        public PhysicsQuaternion Rotation { get; set; }

        /// <inheritdoc/>
        public PhysicsVector3 Velocity { get; set; }

        /// <inheritdoc/>
        public PhysicsVector3 AngularVelocity { get; set; }

        /// <inheritdoc/>
        public float Mass { get; set; }

        /// <inheritdoc/>
        public bool IsKinematic { get; set; }

        /// <inheritdoc/>
        public BodyType BodyType { get; set; }

        /// <inheritdoc/>
        public int InstanceId { get; }

        private static int _nextId;

        /// <summary>
        /// 创建一个新的 MockBody 实例，自动分配 InstanceId
        /// </summary>
        public MockBody()
        {
            InstanceId = ++_nextId;
        }

        /// <inheritdoc/>
        public void AddForce(PhysicsVector3 force)
        {
            LastForce = force;
        }

        /// <inheritdoc/>
        public void AddTorque(PhysicsVector3 torque)
        {
            LastTorque = torque;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Disposed = true;
        }

        /// <inheritdoc/>
        public event Action<CollisionEvent>? OnCollisionEnter;

        /// <inheritdoc/>
        public event Action<CollisionEvent>? OnCollisionStay;

        /// <inheritdoc/>
        public event Action<CollisionEvent>? OnCollisionExit;

        /// <inheritdoc/>
        public event Action<CollisionEvent>? OnTriggerEnter;

        /// <inheritdoc/>
        public event Action<CollisionEvent>? OnTriggerStay;

        /// <inheritdoc/>
        public event Action<CollisionEvent>? OnTriggerExit;

        /// <summary>
        /// 触发 OnCollisionEnter 事件（仅测试用）
        /// </summary>
        public void RaiseCollisionEnter(CollisionEvent evt)
        {
            OnCollisionEnter?.Invoke(evt);
        }
    }

    /// <summary>
    /// IBody 接口合约测试。
    /// 验证接口契约而非具体实现。
    /// </summary>
    [TestFixture]
    public class IBodyContractTests
    {
        private MockBody _body = null!;

        [SetUp]
        public void SetUp()
        {
            ReferencePool.ClearAll();
            _body = new MockBody();
        }

        [TearDown]
        public void TearDown()
        {
            _body.Dispose();
            _body = null!;
            ReferencePool.ClearAll();
        }

        /// <summary>
        /// Position 属性设置后应能正确读取
        /// </summary>
        [Test]
        public void Position_SetAndGet_Roundtrips()
        {
            var pos = new PhysicsVector3(1f, 2f, 3f);
            _body.Position = pos;

            Assert.AreEqual(pos, _body.Position);
        }

        /// <summary>
        /// Rotation 属性设置后应能正确读取
        /// </summary>
        [Test]
        public void Rotation_SetAndGet_Roundtrips()
        {
            var rot = PhysicsQuaternion.Euler(0f, System.MathF.PI * 0.5f, 0f);
            _body.Rotation = rot;

            Assert.AreEqual(rot, _body.Rotation);
        }

        /// <summary>
        /// Velocity 属性设置后应能正确读取
        /// </summary>
        [Test]
        public void Velocity_SetAndGet_Roundtrips()
        {
            var vel = new PhysicsVector3(5f, 0f, 0f);
            _body.Velocity = vel;

            Assert.AreEqual(vel, _body.Velocity);
        }

        /// <summary>
        /// AngularVelocity 属性设置后应能正确读取
        /// </summary>
        [Test]
        public void AngularVelocity_SetAndGet_Roundtrips()
        {
            var angVel = new PhysicsVector3(0f, 1f, 0f);
            _body.AngularVelocity = angVel;

            Assert.AreEqual(angVel, _body.AngularVelocity);
        }

        /// <summary>
        /// Mass 属性设置后应能正确读取
        /// </summary>
        [Test]
        public void Mass_SetAndGet_Roundtrips()
        {
            _body.Mass = 10f;

            Assert.AreEqual(10f, _body.Mass);
        }

        /// <summary>
        /// BodyType 属性设置后应能正确读取
        /// </summary>
        [Test]
        public void BodyType_SetAndGet_Roundtrips()
        {
            _body.BodyType = BodyType.Dynamic;

            Assert.AreEqual(BodyType.Dynamic, _body.BodyType);
        }

        /// <summary>
        /// IsKinematic 属性设置后应能正确读取
        /// </summary>
        [Test]
        public void IsKinematic_SetAndGet_Roundtrips()
        {
            _body.IsKinematic = true;

            Assert.IsTrue(_body.IsKinematic);
        }

        /// <summary>
        /// AddForce 应存储传入的力向量
        /// </summary>
        [Test]
        public void AddForce_StoresForce()
        {
            var force = new PhysicsVector3(0f, 10f, 0f);

            _body.AddForce(force);

            Assert.IsTrue(_body.LastForce.HasValue);
            Assert.AreEqual(force, _body.LastForce!.Value);
        }

        /// <summary>
        /// AddTorque 应存储传入的扭矩向量
        /// </summary>
        [Test]
        public void AddTorque_StoresTorque()
        {
            var torque = new PhysicsVector3(0f, 0f, 5f);

            _body.AddTorque(torque);

            Assert.IsTrue(_body.LastTorque.HasValue);
            Assert.AreEqual(torque, _body.LastTorque!.Value);
        }

        /// <summary>
        /// OnCollisionEnter 事件应可被触发
        /// </summary>
        [Test]
        public void OnCollisionEnter_Event_Raised()
        {
            bool raised = false;
            _body.OnCollisionEnter += evt => { raised = true; };

            _body.RaiseCollisionEnter(new CollisionEvent(
                1, 2, new PhysicsVector3(0f, 0f, 0f), CollisionEventType.Enter));

            Assert.IsTrue(raised);
        }

        /// <summary>
        /// InstanceId 应为每个实例分配唯一值
        /// </summary>
        [Test]
        public void InstanceId_DifferentInstances_HaveDifferentIds()
        {
            var body1 = new MockBody();
            var body2 = new MockBody();

            Assert.That(body1.InstanceId, Is.Not.EqualTo(body2.InstanceId));
        }

        /// <summary>
        /// Dispose 方法应可被调用
        /// </summary>
        [Test]
        public void Dispose_MarksDisposed()
        {
            Assert.IsFalse(_body.Disposed);

            _body.Dispose();

            Assert.IsTrue(_body.Disposed);
        }
    }
}
