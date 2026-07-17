#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Combat;

namespace HN.Framework.Core.Tests.Level.Logic.Combat
{
    [TestFixture]
    public class AttributeTypeTests
    {
        [Test]
        public void Empty_Default_IsInvalid()
        {
            var type = AttributeType.Empty;
            Assert.That(type.IsValid, Is.False);
            Assert.That(type.Index, Is.EqualTo(0));
        }

        [Test]
        public void ConstructedWithIndex_HasCorrectValues()
        {
            var type = new AttributeType(5);
            Assert.That(type.Index, Is.EqualTo(5));
            Assert.That(type.IsValid, Is.True);
        }

        [Test]
        public void Equals_SameIndex_ReturnsTrue()
        {
            var a = new AttributeType(3);
            var b = new AttributeType(3);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
        }

        [Test]
        public void Equals_DifferentIndex_ReturnsFalse()
        {
            var a = new AttributeType(3);
            var b = new AttributeType(5);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void GetHashCode_SameIndex_SameHash()
        {
            var a = new AttributeType(3);
            var b = new AttributeType(3);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void ToString_Empty_ReturnsEmpty()
        {
            Assert.That(AttributeType.Empty.ToString(), Is.EqualTo("Empty"));
        }

        [Test]
        public void ToString_WithoutManager_ReturnsFallback()
        {
            var type = new AttributeType(5);
            var str = type.ToString();
            Assert.That(str, Does.Contain("AttributeType(5)"));
        }
    }
}
