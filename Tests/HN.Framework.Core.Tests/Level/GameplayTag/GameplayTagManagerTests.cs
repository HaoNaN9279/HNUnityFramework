#nullable enable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using GameplayTag = HN.Framework.Core.Level.GameplayTag;
using GameplayTagManager = HN.Framework.Core.Level.GameplayTagManager;
using GameplayTagDefinition = HN.Framework.Core.Level.GameplayTagManager.GameplayTagDefinition;

namespace HN.Framework.Core.Tests.Level.GameplayTagTests
{
    [TestFixture]
    public class GameplayTagManagerTests
    {
        private static List<GameplayTagDefinition> CreateTestDefinitions()
        {
            return new List<GameplayTagDefinition>
            {
                new() { FullName = "State",        Index = 0, ParentIndex = -1, Depth = 1 },
                new() { FullName = "State.Combat", Index = 1, ParentIndex = 0,  Depth = 2 },
                new() { FullName = "State.Combat.Stunned", Index = 2, ParentIndex = 1, Depth = 3 },
                new() { FullName = "Effect",       Index = 3, ParentIndex = -1, Depth = 1 },
                new() { FullName = "Effect.Damage",Index = 4, ParentIndex = 3,  Depth = 2 },
            };
        }

        private GameplayTagManager _manager = null!;
        private List<GameplayTagDefinition> _definitions = null!;

        [SetUp]
        public void SetUp()
        {
            _manager = new GameplayTagManager();
            _definitions = CreateTestDefinitions();
        }

        [Test]
        public void NewManager_IsNotFrozen_CountZero()
        {
            Assert.That(_manager.IsFrozen, Is.False);
            Assert.That(_manager.Count, Is.EqualTo(0));
        }

        [Test]
        public void LoadDefinitions_ValidData_SetsCorrectCount()
        {
            _manager.LoadFromDefinitions(_definitions);
            Assert.That(_manager.Count, Is.EqualTo(5));
        }

        [Test]
        public void LoadDefinitions_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.LoadFromDefinitions(null!));
        }

        [Test]
        public void Freeze_SetsIsFrozen()
        {
            _manager.LoadFromDefinitions(_definitions);
            _manager.Freeze();
            Assert.That(_manager.IsFrozen, Is.True);
        }

        [Test]
        public void Freeze_SetsCurrent()
        {
            _manager.LoadFromDefinitions(_definitions);
            _manager.Freeze();
            Assert.That(GameplayTagManager.Current, Is.Not.Null);
        }

        [Test]
        public void LoadDefinitions_AfterFreeze_ThrowsInvalidOperationException()
        {
            _manager.LoadFromDefinitions(_definitions);
            _manager.Freeze();
            Assert.Throws<InvalidOperationException>(() => _manager.LoadFromDefinitions(_definitions));
        }

        [Test]
        public void GetTag_ValidName_ReturnsCorrectTag()
        {
            _manager.LoadFromDefinitions(_definitions);
            var tag = _manager.GetTag("State.Combat");
            Assert.That(tag.TableIndex, Is.EqualTo(2));
        }

        [Test]
        public void GetTag_InvalidName_ThrowsKeyNotFoundException()
        {
            _manager.LoadFromDefinitions(_definitions);
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
                () => _manager.GetTag("NotExist"));
        }

        [Test]
        public void TryGetTag_ValidName_ReturnsTrue()
        {
            _manager.LoadFromDefinitions(_definitions);
            bool found = _manager.TryGetTag("Effect", out var tag);
            Assert.That(found, Is.True);
            Assert.That(tag.TableIndex, Is.EqualTo(4));
        }

        [Test]
        public void TryGetTag_InvalidName_ReturnsFalse()
        {
            _manager.LoadFromDefinitions(_definitions);
            bool found = _manager.TryGetTag("NotExist", out var tag);
            Assert.That(found, Is.False);
            Assert.That(tag.IsValid, Is.False);
        }

        [Test]
        public void GetTagDepth_ValidTag_ReturnsCorrectDepth()
        {
            _manager.LoadFromDefinitions(_definitions);
            var state = _manager.GetTag("State");
            var combat = _manager.GetTag("State.Combat");
            var stunned = _manager.GetTag("State.Combat.Stunned");

            Assert.That(_manager.GetTagDepth(state), Is.EqualTo(1));
            Assert.That(_manager.GetTagDepth(combat), Is.EqualTo(2));
            Assert.That(_manager.GetTagDepth(stunned), Is.EqualTo(3));
        }

        [Test]
        public void IsDescendantOf_DirectParent_ReturnsTrue()
        {
            _manager.LoadFromDefinitions(_definitions);
            _manager.Freeze();

            var stunned = _manager.GetTag("State.Combat.Stunned");
            var state = _manager.GetTag("State");

            Assert.That(GameplayTagManager.IsDescendantOf(stunned, state), Is.True);
        }

        [Test]
        public void IsDescendantOf_NotRelated_ReturnsFalse()
        {
            _manager.LoadFromDefinitions(_definitions);
            _manager.Freeze();

            var stunned = _manager.GetTag("State.Combat.Stunned");
            var effect = _manager.GetTag("Effect");

            Assert.That(GameplayTagManager.IsDescendantOf(stunned, effect), Is.False);
        }

        [Test]
        public void GetTagName_ValidTag_ReturnsName()
        {
            _manager.LoadFromDefinitions(_definitions);
            _manager.Freeze();

            var stunned = _manager.GetTag("State.Combat.Stunned");
            var name = GameplayTagManager.GetTagName(stunned);

            Assert.That(name, Is.EqualTo("State.Combat.Stunned"));
        }
    }
}
