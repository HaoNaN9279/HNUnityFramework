#nullable enable

using NUnit.Framework;
using GameplayTag = HN.Framework.Core.Level.GameplayTag;

namespace HN.Framework.Core.Tests.Level.GameplayTagTests
{
    [TestFixture]
    public class GameplayTagTests
    {
        [Test]
        public void Empty_Default_IsInvalid()
        {
            var tag = GameplayTag.Empty;
            Assert.That(tag.IsValid, Is.False);
            Assert.That(tag.TableIndex, Is.EqualTo(0));
        }

        [Test]
        public void ConstructedWithIndex_HasCorrectValues()
        {
            var tag = new GameplayTag(5, 1);
            Assert.That(tag.TableIndex, Is.EqualTo(5));
            Assert.That(tag.InstanceId, Is.EqualTo(1));
        }

        [Test]
        public void Equals_SameTableIndex_ReturnsTrue()
        {
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(5, 1);
            Assert.That(tagA.Equals(tagB), Is.True);
        }

        [Test]
        public void Equals_DifferentTableIndex_ReturnsFalse()
        {
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(3, 0);
            Assert.That(tagA.Equals(tagB), Is.False);
        }

        [Test]
        public void Equals_Object_SameTableIndex_ReturnsTrue()
        {
            var tagA = new GameplayTag(5, 0);
            object tagB = new GameplayTag(5, 1);
            Assert.That(tagA.Equals(tagB), Is.True);
        }

        [Test]
        public void GetHashCode_SameIndex_SameHash()
        {
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(5, 1);
            Assert.That(tagA.GetHashCode(), Is.EqualTo(tagB.GetHashCode()));
        }

        [Test]
        public void OperatorEquals_SameIndex_ReturnsTrue()
        {
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(5, 1);
            Assert.That(tagA == tagB, Is.True);
        }

        [Test]
        public void OperatorNotEquals_DifferentIndex_ReturnsTrue()
        {
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(3, 0);
            Assert.That(tagA != tagB, Is.True);
        }

        [Test]
        public void MatchesExact_SameIndex_ReturnsTrue()
        {
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(5, 0);
            Assert.That(tagA.MatchesExact(tagB), Is.True);
        }

        [Test]
        public void MatchesExact_DifferentIndex_ReturnsFalse()
        {
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(3, 0);
            Assert.That(tagA.MatchesExact(tagB), Is.False);
        }

        [Test]
        public void Matches_WithoutManager_ReturnsFalse()
        {
            // 未初始化 Manager 时 Matches 应安全返回 false
            var tagA = new GameplayTag(5, 0);
            var tagB = new GameplayTag(3, 0);
            Assert.That(tagA.Matches(tagB), Is.False);
        }

        [Test]
        public void ToString_WithoutManager_ReturnsFallback()
        {
            var tag = new GameplayTag(5, 0);
            var str = tag.ToString();
            Assert.That(str, Does.Contain("GameplayTag(5)"));
        }

        [Test]
        public void ToString_Empty_ReturnsEmpty()
        {
            var str = GameplayTag.Empty.ToString();
            Assert.That(str, Is.EqualTo("Empty"));
        }
    }
}
