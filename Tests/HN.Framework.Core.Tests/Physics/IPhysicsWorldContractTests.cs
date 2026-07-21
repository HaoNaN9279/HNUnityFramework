#nullable enable

using System.Collections.Generic;
using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Physics
{
    /// <summary>
    /// IPhysicsWorld 接口的模拟实现，用于合约测试。
    /// </summary>
    internal sealed class MockPhysicsWorld : IPhysicsWorld
    {
        private readonly PhysicsDimension _dimension;
        private readonly List<MockBody> _bodies = new();

        /// <summary>
        /// 获取创建过的所有刚体
        /// </summary>
        public IReadOnlyList<MockBody> Bodies => _bodies;

        /// <summary>
        /// 获取 Dispose 是否被调用
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// 获取 Step 被调用的总次数
        /// </summary>
        public int StepCallCount { get; private set; }

        /// <summary>
        /// 获取上次 Step 调用的 deltaTime
        /// </summary>
        public float LastDeltaTime { get; private set; }

        /// <summary>
        /// 创建一个新的 MockPhysicsWorld 实例
        /// </summary>
        /// <param name="dimension">物理维度</param>
        public MockPhysicsWorld(PhysicsDimension dimension = PhysicsDimension.D3)
        {
            _dimension = dimension;
        }

        /// <inheritdoc/>
        public PhysicsDimension Dimension => _dimension;

        /// <inheritdoc/>
        public IBody CreateBody(ShapeDefinition shape, PhysicsVector3 position, PhysicsQuaternion rotation, BodyType bodyType, string layer)
        {
            var body = new MockBody
            {
                Position = position,
                Rotation = rotation,
                BodyType = bodyType,
            };
            _bodies.Add(body);
            return body;
        }

        /// <inheritdoc/>
        public void DestroyBody(IBody body)
        {
            if (body is MockBody mockBody)
            {
                _bodies.Remove(mockBody);
                mockBody.Dispose();
            }
        }

        /// <inheritdoc/>
        public bool Raycast(PhysicsVector3 origin, PhysicsVector3 direction, out RaycastHit hitInfo, float maxDistance, string layerMask)
        {
            hitInfo = default;
            return false;
        }

        /// <inheritdoc/>
        public RaycastHit[] RaycastAll(PhysicsVector3 origin, PhysicsVector3 direction, float maxDistance, string layerMask)
        {
            return System.Array.Empty<RaycastHit>();
        }

        /// <inheritdoc/>
        public RaycastHit[] OverlapSphere(PhysicsVector3 center, float radius, string layerMask)
        {
            return System.Array.Empty<RaycastHit>();
        }

        /// <inheritdoc/>
        public RaycastHit[] OverlapBox(PhysicsVector3 center, PhysicsVector3 halfExtents, PhysicsQuaternion rotation, string layerMask)
        {
            return System.Array.Empty<RaycastHit>();
        }

        /// <inheritdoc/>
        public void Step(float deltaTime)
        {
            StepCallCount++;
            LastDeltaTime = deltaTime;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Disposed = true;
        }
    }

    /// <summary>
    /// IPhysicsWorld 接口合约测试。
    /// 验证接口契约而非具体实现。
    /// </summary>
    [TestFixture]
    public class IPhysicsWorldContractTests
    {
        private MockPhysicsWorld _world = null!;

        [SetUp]
        public void SetUp()
        {
            ReferencePool.ClearAll();
            _world = new MockPhysicsWorld();
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
            _world = null!;
            ReferencePool.ClearAll();
        }

        /// <summary>
        /// Dimension 属性应返回构造时传入的维度
        /// </summary>
        [Test]
        public void Dimension_ReturnsExpected()
        {
            Assert.AreEqual(PhysicsDimension.D3, _world.Dimension);
        }

        /// <summary>
        /// 指定维度构造后 Dimension 应正确
        /// </summary>
        [Test]
        public void Dimension_ConstructedWithD2_ReturnsD2()
        {
            var world2D = new MockPhysicsWorld(PhysicsDimension.D2);
            Assert.AreEqual(PhysicsDimension.D2, world2D.Dimension);
        }

        /// <summary>
        /// CreateBody 应返回非空的 IBody 实例
        /// </summary>
        [Test]
        public void CreateBody_ReturnsBody()
        {
            var shape = new BoxShape();
            var pos = new PhysicsVector3(0f, 0f, 0f);
            var rot = PhysicsQuaternion.Identity;

            var body = _world.CreateBody(shape, pos, rot, BodyType.Dynamic, "Default");

            Assert.IsNotNull(body);
            Assert.AreEqual(pos, body.Position);
            Assert.AreEqual(rot, body.Rotation);
            Assert.AreEqual(BodyType.Dynamic, body.BodyType);
        }

        /// <summary>
        /// CreateBody 后 DestroyBody 应能正确清理刚体
        /// </summary>
        [Test]
        public void CreateAndDestroyBody_Lifecycle()
        {
            var shape = new BoxShape();
            var body = _world.CreateBody(shape, new PhysicsVector3(0f, 0f, 0f), PhysicsQuaternion.Identity, BodyType.Static, "Default");

            Assert.That(_world.Bodies.Count, Is.EqualTo(1));

            _world.DestroyBody(body);

            Assert.That(_world.Bodies.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 空世界中的 Raycast 应返回 false
        /// </summary>
        [Test]
        public void Raycast_NoHit_ReturnsFalse()
        {
            var origin = new PhysicsVector3(0f, 0f, 0f);
            var direction = new PhysicsVector3(0f, 0f, 1f);

            bool hit = _world.Raycast(origin, direction, out var hitInfo, 100f, "Default");

            Assert.IsFalse(hit);
            Assert.AreEqual(default(RaycastHit), hitInfo);
        }

        /// <summary>
        /// RaycastAll 空世界应返回空数组
        /// </summary>
        [Test]
        public void RaycastAll_EmptyWorld_ReturnsEmptyArray()
        {
            var result = _world.RaycastAll(
                new PhysicsVector3(0f, 0f, 0f),
                new PhysicsVector3(0f, 0f, 1f),
                100f,
                "Default");

            Assert.IsNotNull(result);
            Assert.That(result.Length, Is.EqualTo(0));
        }

        /// <summary>
        /// Step 方法调用不应抛出异常
        /// </summary>
        [Test]
        public void Step_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _world.Step(0.016f));
        }

        /// <summary>
        /// Step 调用后计数应增加
        /// </summary>
        [Test]
        public void Step_IncrementsCallCount()
        {
            _world.Step(0.016f);
            _world.Step(0.016f);

            Assert.That(_world.StepCallCount, Is.EqualTo(2));
        }

        /// <summary>
        /// Step 应记录传入的 deltaTime
        /// </summary>
        [Test]
        public void Step_RecordsDeltaTime()
        {
            _world.Step(0.033f);

            Assert.AreEqual(0.033f, _world.LastDeltaTime);
        }

        /// <summary>
        /// Dispose 方法应可被调用
        /// </summary>
        [Test]
        public void Dispose_MarksDisposed()
        {
            Assert.IsFalse(_world.Disposed);

            _world.Dispose();

            Assert.IsTrue(_world.Disposed);
        }
    }
}
