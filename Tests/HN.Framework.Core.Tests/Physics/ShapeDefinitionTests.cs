#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Physics
{
    [TestFixture]
    public class ShapeDefinitionTests
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

        // ============ BoxShape Tests ============

        /// <summary>
        /// 默认构造的 BoxShape 半尺寸应为 (0.5, 0.5, 0.5)
        /// </summary>
        [Test]
        public void BoxShape_DefaultConstructor_SetsDefaultHalfExtents()
        {
            var box = new BoxShape();

            Assert.AreEqual(0.5f, box.HalfExtents.X);
            Assert.AreEqual(0.5f, box.HalfExtents.Y);
            Assert.AreEqual(0.5f, box.HalfExtents.Z);
        }

        /// <summary>
        /// 参数化构造函数应正确设置半尺寸
        /// </summary>
        [Test]
        public void BoxShape_ParameterizedConstructor_SetsHalfExtents()
        {
            var halfExtents = new PhysicsVector3(1f, 2f, 3f);

            var box = new BoxShape(halfExtents);

            Assert.AreEqual(halfExtents, box.HalfExtents);
        }

        /// <summary>
        /// Clear 方法应将 BoxShape 重置为默认半尺寸
        /// </summary>
        [Test]
        public void BoxShape_Clear_ResetsToDefaults()
        {
            var box = new BoxShape(new PhysicsVector3(3f, 3f, 3f));

            box.Clear();

            Assert.AreEqual(0.5f, box.HalfExtents.X);
            Assert.AreEqual(0.5f, box.HalfExtents.Y);
            Assert.AreEqual(0.5f, box.HalfExtents.Z);
        }

        // ============ SphereShape Tests ============

        /// <summary>
        /// 默认构造的 SphereShape 半径应为 0.5
        /// </summary>
        [Test]
        public void SphereShape_DefaultConstructor_SetsDefaultRadius()
        {
            var sphere = new SphereShape();

            Assert.AreEqual(0.5f, sphere.Radius);
        }

        /// <summary>
        /// 参数化构造函数应正确设置半径
        /// </summary>
        [Test]
        public void SphereShape_ParameterizedConstructor_SetsRadius()
        {
            var sphere = new SphereShape(2.5f);

            Assert.AreEqual(2.5f, sphere.Radius);
        }

        /// <summary>
        /// Clear 方法应将 SphereShape 重置为默认半径
        /// </summary>
        [Test]
        public void SphereShape_Clear_ResetsToDefaults()
        {
            var sphere = new SphereShape(5f);

            sphere.Clear();

            Assert.AreEqual(0.5f, sphere.Radius);
        }

        // ============ CapsuleShape Tests ============

        /// <summary>
        /// 默认构造的 CapsuleShape 半径应为 0.5，高度应为 2
        /// </summary>
        [Test]
        public void CapsuleShape_DefaultConstructor_SetsDefaults()
        {
            var capsule = new CapsuleShape();

            Assert.AreEqual(0.5f, capsule.Radius);
            Assert.AreEqual(2f, capsule.Height);
        }

        /// <summary>
        /// 参数化构造函数应正确设置半径和高度
        /// </summary>
        [Test]
        public void CapsuleShape_ParameterizedConstructor_SetsProperties()
        {
            var capsule = new CapsuleShape(1f, 3f);

            Assert.AreEqual(1f, capsule.Radius);
            Assert.AreEqual(3f, capsule.Height);
        }

        /// <summary>
        /// Clear 方法应将 CapsuleShape 重置为默认值
        /// </summary>
        [Test]
        public void CapsuleShape_Clear_ResetsToDefaults()
        {
            var capsule = new CapsuleShape(3f, 10f);

            capsule.Clear();

            Assert.AreEqual(0.5f, capsule.Radius);
            Assert.AreEqual(2f, capsule.Height);
        }
    }
}
