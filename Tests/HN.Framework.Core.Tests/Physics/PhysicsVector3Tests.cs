#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Physics
{
    [TestFixture]
    public class PhysicsVector3Tests
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
        /// 默认构造的 PhysicsVector3 所有分量应为零
        /// </summary>
        [Test]
        public void DefaultValues_AreZero()
        {
            PhysicsVector3 v = default;

            Assert.AreEqual(0f, v.X);
            Assert.AreEqual(0f, v.Y);
            Assert.AreEqual(0f, v.Z);
        }

        // ============ Constructor ============

        /// <summary>
        /// 构造函数应正确设置各个分量
        /// </summary>
        [Test]
        public void Constructor_SetsComponents()
        {
            var v = new PhysicsVector3(1f, 2f, 3f);

            Assert.AreEqual(1f, v.X);
            Assert.AreEqual(2f, v.Y);
            Assert.AreEqual(3f, v.Z);
        }

        // ============ Operator Tests ============

        /// <summary>
        /// 加法运算符应逐分量相加
        /// </summary>
        [Test]
        public void Addition_AddsComponents()
        {
            var a = new PhysicsVector3(1f, 2f, 3f);
            var b = new PhysicsVector3(4f, 5f, 6f);

            var result = a + b;

            Assert.AreEqual(5f, result.X);
            Assert.AreEqual(7f, result.Y);
            Assert.AreEqual(9f, result.Z);
        }

        /// <summary>
        /// 减法运算符应逐分量相减
        /// </summary>
        [Test]
        public void Subtraction_SubtractsComponents()
        {
            var a = new PhysicsVector3(5f, 7f, 9f);
            var b = new PhysicsVector3(1f, 2f, 3f);

            var result = a - b;

            Assert.AreEqual(4f, result.X);
            Assert.AreEqual(5f, result.Y);
            Assert.AreEqual(6f, result.Z);
        }

        /// <summary>
        /// 标量乘法应逐分量乘以标量
        /// </summary>
        [Test]
        public void ScalarMultiplication_MultipliesEachComponent()
        {
            var v = new PhysicsVector3(1f, 2f, 3f);

            var result = v * 2f;

            Assert.AreEqual(2f, result.X);
            Assert.AreEqual(4f, result.Y);
            Assert.AreEqual(6f, result.Z);
        }

        /// <summary>
        /// 标量除法应逐分量除以标量
        /// </summary>
        [Test]
        public void ScalarDivision_DividesEachComponent()
        {
            var v = new PhysicsVector3(2f, 4f, 6f);

            var result = v / 2f;

            Assert.AreEqual(1f, result.X);
            Assert.AreEqual(2f, result.Y);
            Assert.AreEqual(3f, result.Z);
        }

        /// <summary>
        /// 相同分量值的两个 PhysicsVector3 应相等
        /// </summary>
        [Test]
        public void Equality_SameValues_ReturnsTrue()
        {
            var a = new PhysicsVector3(1f, 2f, 3f);
            var b = new PhysicsVector3(1f, 2f, 3f);

            Assert.IsTrue(a == b);
            Assert.IsTrue(a.Equals(b));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        /// <summary>
        /// 不同分量值的两个 PhysicsVector3 应不相等
        /// </summary>
        [Test]
        public void Inequality_DifferentValues_ReturnsTrue()
        {
            var a = new PhysicsVector3(1f, 2f, 3f);
            var b = new PhysicsVector3(4f, 5f, 6f);

            Assert.IsTrue(a != b);
            Assert.IsFalse(a.Equals(b));
        }

        // ============ Static Properties ============

        /// <summary>
        /// Zero 静态属性应返回所有分量为零的向量
        /// </summary>
        [Test]
        public void Zero_ReturnsZeroVector()
        {
            var zero = PhysicsVector3.Zero;

            Assert.AreEqual(0f, zero.X);
            Assert.AreEqual(0f, zero.Y);
            Assert.AreEqual(0f, zero.Z);
        }

        /// <summary>
        /// Up 静态属性应返回 (0, 1, 0)
        /// </summary>
        [Test]
        public void Up_ReturnsUnitY()
        {
            var up = PhysicsVector3.Up;

            Assert.AreEqual(0f, up.X);
            Assert.AreEqual(1f, up.Y);
            Assert.AreEqual(0f, up.Z);
        }

        /// <summary>
        /// Forward 静态属性应返回 (0, 0, 1)
        /// </summary>
        [Test]
        public void Forward_ReturnsUnitZ()
        {
            var forward = PhysicsVector3.Forward;

            Assert.AreEqual(0f, forward.X);
            Assert.AreEqual(0f, forward.Y);
            Assert.AreEqual(1f, forward.Z);
        }

        /// <summary>
        /// Right 静态属性应返回 (1, 0, 0)
        /// </summary>
        [Test]
        public void Right_ReturnsUnitX()
        {
            var right = PhysicsVector3.Right;

            Assert.AreEqual(1f, right.X);
            Assert.AreEqual(0f, right.Y);
            Assert.AreEqual(0f, right.Z);
        }

        // ============ Methods ============

        /// <summary>
        /// Magnitude 应返回向量的正确长度
        /// </summary>
        [Test]
        public void Magnitude_ReturnsCorrectLength()
        {
            var v = new PhysicsVector3(3f, 4f, 0f);

            Assert.AreEqual(5f, v.Magnitude);
        }

        /// <summary>
        /// Dot 应返回两个向量的正确点积值
        /// </summary>
        [Test]
        public void Dot_ReturnsCorrectValue()
        {
            var a = new PhysicsVector3(1f, 2f, 3f);
            var b = new PhysicsVector3(4f, 5f, 6f);

            float dot = PhysicsVector3.Dot(a, b);

            Assert.AreEqual(32f, dot); // 1*4 + 2*5 + 3*6 = 32
        }

        /// <summary>
        /// Cross 应返回垂直于两个输入向量的向量
        /// </summary>
        [Test]
        public void Cross_ReturnsPerpendicular()
        {
            var a = new PhysicsVector3(1f, 0f, 0f);
            var b = new PhysicsVector3(0f, 1f, 0f);

            var result = PhysicsVector3.Cross(a, b);

            Assert.AreEqual(0f, result.X);
            Assert.AreEqual(0f, result.Y);
            Assert.AreEqual(1f, result.Z);
        }

        /// <summary>
        /// Distance 应返回两点之间的正确欧几里得距离
        /// </summary>
        [Test]
        public void Distance_ReturnsCorrectValue()
        {
            var a = new PhysicsVector3(0f, 0f, 0f);
            var b = new PhysicsVector3(3f, 4f, 0f);

            float distance = PhysicsVector3.Distance(a, b);

            Assert.AreEqual(5f, distance);
        }

        /// <summary>
        /// Normalized 应将向量归一化为单位长度
        /// </summary>
        [Test]
        public void Normalized_ReturnsUnitVector()
        {
            var v = new PhysicsVector3(3f, 4f, 0f);

            var normalized = v.Normalized;

            Assert.AreEqual(0.6f, normalized.X, 1e-6f);
            Assert.AreEqual(0.8f, normalized.Y, 1e-6f);
            Assert.AreEqual(0f, normalized.Z, 1e-6f);
            Assert.AreEqual(1f, normalized.Magnitude, 1e-6f);
        }

        /// <summary>
        /// Lerp 应在两个向量之间正确插值
        /// </summary>
        [Test]
        public void Lerp_InterpolatesCorrectly()
        {
            var a = new PhysicsVector3(0f, 0f, 0f);
            var b = new PhysicsVector3(10f, 10f, 10f);

            var result = PhysicsVector3.Lerp(a, b, 0.5f);

            Assert.AreEqual(5f, result.X);
            Assert.AreEqual(5f, result.Y);
            Assert.AreEqual(5f, result.Z);
        }

        // ============ ToString ============

        /// <summary>
        /// ToString 应返回包含分量信息的格式化字符串
        /// </summary>
        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            var v = new PhysicsVector3(1f, 2f, 3f);

            string result = v.ToString();

            Assert.That(result, Does.Contain("1"));
            Assert.That(result, Does.Contain("2"));
            Assert.That(result, Does.Contain("3"));
        }
    }
}
