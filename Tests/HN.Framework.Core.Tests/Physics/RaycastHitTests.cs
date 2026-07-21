#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Physics
{
    [TestFixture]
    public class RaycastHitTests
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

        // ============ Default Values ============

        /// <summary>
        /// 默认构造的 RaycastHit 所有字段应为默认值
        /// </summary>
        [Test]
        public void DefaultValues_AreDefault()
        {
            RaycastHit hit = default;

            Assert.AreEqual(default(PhysicsVector3), hit.Point);
            Assert.AreEqual(default(PhysicsVector3), hit.Normal);
            Assert.AreEqual(0f, hit.Distance);
            Assert.AreEqual(0, hit.ColliderInstanceId);
        }

        // ============ Constructor ============

        /// <summary>
        /// 构造函数应正确存储所有属性值
        /// </summary>
        [Test]
        public void Constructor_SetsProperties()
        {
            var point = new PhysicsVector3(1f, 2f, 3f);
            var normal = new PhysicsVector3(0f, 1f, 0f);

            var hit = new RaycastHit(point, normal, 10.5f, 42);

            Assert.AreEqual(point, hit.Point);
            Assert.AreEqual(normal, hit.Normal);
            Assert.AreEqual(10.5f, hit.Distance);
            Assert.AreEqual(42, hit.ColliderInstanceId);
        }

        // ============ Equality ============

        /// <summary>
        /// 相同字段值的两个 RaycastHit 应相等
        /// </summary>
        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var point = new PhysicsVector3(1f, 2f, 3f);
            var normal = new PhysicsVector3(0f, 1f, 0f);
            var hit1 = new RaycastHit(point, normal, 10f, 1);
            var hit2 = new RaycastHit(point, normal, 10f, 1);

            Assert.IsTrue(hit1 == hit2);
            Assert.IsTrue(hit1.Equals(hit2));
            Assert.AreEqual(hit1.GetHashCode(), hit2.GetHashCode());
        }

        /// <summary>
        /// 不同字段值的两个 RaycastHit 应不相等
        /// </summary>
        [Test]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var hit1 = new RaycastHit(
                new PhysicsVector3(1f, 2f, 3f),
                new PhysicsVector3(0f, 1f, 0f),
                10f,
                1);
            var hit2 = new RaycastHit(
                new PhysicsVector3(4f, 5f, 6f),
                new PhysicsVector3(0f, 0f, 1f),
                20f,
                2);

            Assert.IsFalse(hit1 == hit2);
            Assert.IsTrue(hit1 != hit2);
            Assert.IsFalse(hit1.Equals(hit2));
        }

        /// <summary>
        /// Object.Equals 应正确处理非 RaycastHit 类型和 null
        /// </summary>
        [Test]
        public void Equals_NonRaycastHit_ReturnsFalse()
        {
            var hit = new RaycastHit(
                new PhysicsVector3(1f, 2f, 3f),
                new PhysicsVector3(0f, 1f, 0f),
                10f,
                1);

            Assert.IsFalse(hit.Equals(null));
            Assert.IsFalse(hit.Equals("string"));
        }

        // ============ ToString ============

        /// <summary>
        /// ToString 应返回包含字段信息的格式化字符串
        /// </summary>
        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            var hit = new RaycastHit(
                new PhysicsVector3(1f, 2f, 3f),
                new PhysicsVector3(0f, 1f, 0f),
                10f,
                42);

            string result = hit.ToString();

            Assert.That(result, Does.Contain("RaycastHit"), "应包含类型名");
            Assert.That(result, Does.Contain("42"), "应包含 ColliderInstanceId");
        }
    }
}
