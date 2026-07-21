#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Physics
{
    [TestFixture]
    public class PhysicsTypesTests
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
        /// PhysicsDimension 枚举应包含 D2、D3 两个值
        /// </summary>
        [Test]
        public void PhysicsDimension_HasExpectedMembers()
        {
            var names = System.Enum.GetNames(typeof(PhysicsDimension));
            Assert.AreEqual(2, names.Length);
            Assert.Contains(nameof(PhysicsDimension.D2), names);
            Assert.Contains(nameof(PhysicsDimension.D3), names);
        }

        /// <summary>
        /// BodyType 枚举应包含 Static、Dynamic、Kinematic 三个值
        /// </summary>
        [Test]
        public void BodyType_HasExpectedMembers()
        {
            var names = System.Enum.GetNames(typeof(BodyType));
            Assert.AreEqual(3, names.Length);
            Assert.Contains(nameof(BodyType.Static), names);
            Assert.Contains(nameof(BodyType.Dynamic), names);
            Assert.Contains(nameof(BodyType.Kinematic), names);
        }
    }
}
