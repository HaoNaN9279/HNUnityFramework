#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Physics
{
    [TestFixture]
    public class CollisionEventTests
    {
        [SetUp]
        public void SetUp()
        {
            ReferencePool.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            ReferencePool.ClearAll();
        }

        // ============ Enum Tests ============

        /// <summary>
        /// CollisionEventType 枚举应包含 Enter、Stay、Exit 三个值
        /// </summary>
        [Test]
        public void CollisionEventType_HasExpectedMembers()
        {
            var names = System.Enum.GetNames(typeof(CollisionEventType));
            Assert.AreEqual(3, names.Length);
            Assert.Contains(nameof(CollisionEventType.Enter), names);
            Assert.Contains(nameof(CollisionEventType.Stay), names);
            Assert.Contains(nameof(CollisionEventType.Exit), names);
        }

        // ============ Default Values ============

        /// <summary>
        /// 默认构造的 CollisionEvent 所有字段应为默认值
        /// </summary>
        [Test]
        public void DefaultValues_AreDefault()
        {
            CollisionEvent evt = default;

            Assert.AreEqual(0, evt.SelfInstanceId);
            Assert.AreEqual(0, evt.OtherInstanceId);
            Assert.AreEqual(default(PhysicsVector3), evt.RelativeVelocity);
            Assert.AreEqual(default(CollisionEventType), evt.Type);
        }

        // ============ Constructor ============

        /// <summary>
        /// 构造函数应正确存储所有属性值
        /// </summary>
        [Test]
        public void Constructor_SetsProperties()
        {
            var relVel = new PhysicsVector3(1f, 0f, 0f);

            var evt = new CollisionEvent(1, 2, relVel, CollisionEventType.Enter);

            Assert.AreEqual(1, evt.SelfInstanceId);
            Assert.AreEqual(2, evt.OtherInstanceId);
            Assert.AreEqual(relVel, evt.RelativeVelocity);
            Assert.AreEqual(CollisionEventType.Enter, evt.Type);
        }

        // ============ Equality ============

        /// <summary>
        /// 相同字段值的两个 CollisionEvent 应相等
        /// </summary>
        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var relVel = new PhysicsVector3(1f, 0f, 0f);
            var evt1 = new CollisionEvent(1, 2, relVel, CollisionEventType.Stay);
            var evt2 = new CollisionEvent(1, 2, relVel, CollisionEventType.Stay);

            Assert.IsTrue(evt1 == evt2);
            Assert.IsTrue(evt1.Equals(evt2));
            Assert.AreEqual(evt1.GetHashCode(), evt2.GetHashCode());
        }

        /// <summary>
        /// 不同字段值的两个 CollisionEvent 应不相等
        /// </summary>
        [Test]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var evt1 = new CollisionEvent(1, 2, new PhysicsVector3(0f, 0f, 0f), CollisionEventType.Enter);
            var evt2 = new CollisionEvent(3, 4, new PhysicsVector3(1f, 0f, 0f), CollisionEventType.Exit);

            Assert.IsFalse(evt1 == evt2);
            Assert.IsTrue(evt1 != evt2);
            Assert.IsFalse(evt1.Equals(evt2));
        }

        /// <summary>
        /// Object.Equals 应正确处理非 CollisionEvent 类型和 null
        /// </summary>
        [Test]
        public void Equals_NonCollisionEvent_ReturnsFalse()
        {
            var evt = new CollisionEvent(1, 2, new PhysicsVector3(0f, 0f, 0f), CollisionEventType.Enter);

            Assert.IsFalse(evt.Equals(null));
            Assert.IsFalse(evt.Equals("string"));
        }

        // ============ ToString ============

        /// <summary>
        /// ToString 应返回包含字段信息的格式化字符串
        /// </summary>
        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            var evt = new CollisionEvent(1, 2, new PhysicsVector3(0f, 0f, 0f), CollisionEventType.Exit);

            string result = evt.ToString();

            Assert.That(result, Does.Contain("CollisionEvent"), "应包含类型名");
            Assert.That(result, Does.Contain("1"), "应包含 SelfInstanceId");
            Assert.That(result, Does.Contain("Exit"), "应包含事件类型");
        }
    }
}
