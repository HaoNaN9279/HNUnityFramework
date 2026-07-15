#nullable enable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using GameplayTag = HN.Framework.Core.Level.GameplayTag;
using GameplayTagManager = HN.Framework.Core.Level.GameplayTagManager;
using GameplayTagContainer = HN.Framework.Core.Level.GameplayTagContainer;
using GameplayTagDefinition = HN.Framework.Core.Level.GameplayTagManager.GameplayTagDefinition;

namespace HN.Framework.Core.Tests.Level.GameplayTagTests
{
    /// <summary>
    /// <see cref="GameplayTagContainer"/> 的单元测试。
    /// 测试标签容器的增删查、批量操作和层级匹配行为。
    /// </summary>
    [TestFixture]
    public class GameplayTagContainerTests
    {
        private GameplayTagManager _manager = null!;

        // 测试用标签常量（TableIndex 对应定义表中的索引位置）
        // 1=State, 2=State.Combat, 3=State.Combat.Stunned
        // 4=Effect, 5=Effect.Damage, 6=Effect.Buff
        private static GameplayTag TagState => new(1, 0);
        private static GameplayTag TagCombat => new(2, 0);
        private static GameplayTag TagStunned => new(3, 0);
        private static GameplayTag TagEffect => new(4, 0);
        private static GameplayTag TagDamage => new(5, 0);
        private static GameplayTag TagBuff => new(6, 0);

        [SetUp]
        public void SetUp()
        {
            _manager = new GameplayTagManager();
            _manager.LoadFromDefinitions(new List<GameplayTagDefinition>
            {
                new() { FullName = "State",               Index = 0, ParentIndex = -1, Depth = 1 },
                new() { FullName = "State.Combat",        Index = 1, ParentIndex = 0,  Depth = 2 },
                new() { FullName = "State.Combat.Stunned",Index = 2, ParentIndex = 1,  Depth = 3 },
                new() { FullName = "Effect",              Index = 3, ParentIndex = -1, Depth = 1 },
                new() { FullName = "Effect.Damage",       Index = 4, ParentIndex = 3,  Depth = 2 },
                new() { FullName = "Effect.Buff",         Index = 5, ParentIndex = 3,  Depth = 2 },
            });
            _manager.Freeze();
        }

        [TearDown]
        public void TearDown()
        {
            // 由于 GameplayTagManager.Current 是 internal static 且仅有 getter，
            // 无法在 TearDown 中将其重置为 null。
            // 每个测试通过自己的 SetUp/Freeze 设置 Current，互不干扰。
        }

        // ============ 基础操作测试 ============

        /// <summary>
        /// 新建的容器应为空。
        /// </summary>
        [Test]
        public void NewContainer_IsEmpty()
        {
            var container = new GameplayTagContainer();

            Assert.That(container.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 添加有效标签后 Count 应增加。
        /// </summary>
        [Test]
        public void AddTag_ValidTag_IncreasesCount()
        {
            var container = new GameplayTagContainer();

            container.AddTag(TagStunned);

            Assert.That(container.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// 添加无效标签（Empty）不应增加 Count。
        /// </summary>
        [Test]
        public void AddTag_EmptyTag_DoesNotAdd()
        {
            var container = new GameplayTagContainer();

            container.AddTag(GameplayTag.Empty);

            Assert.That(container.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 重复添加相同标签不应增加 Count。
        /// </summary>
        [Test]
        public void AddTag_Duplicate_DoesNotIncreaseCount()
        {
            var container = new GameplayTagContainer();

            container.AddTag(TagStunned);
            container.AddTag(TagStunned);

            Assert.That(container.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// 移除已存在的标签后 Count 应减少。
        /// </summary>
        [Test]
        public void RemoveTag_Existing_DecreasesCount()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            container.RemoveTag(TagStunned);

            Assert.That(container.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 移除不存在的标签时 Count 应不变。
        /// </summary>
        [Test]
        public void RemoveTag_NonExisting_NoChange()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            container.RemoveTag(TagEffect);

            Assert.That(container.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// 清空非空容器后 Count 应归零。
        /// </summary>
        [Test]
        public void Clear_NonEmpty_ResetsToZero()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);
            container.AddTag(TagEffect);

            container.Clear();

            Assert.That(container.Count, Is.EqualTo(0));
        }

        // ============ 严格匹配测试 ============

        /// <summary>
        /// 严格匹配存在的标签应返回 true。
        /// </summary>
        [Test]
        public void HasTagExact_Exists_ReturnsTrue()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = container.HasTagExact(TagStunned);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// 严格匹配不存在的标签应返回 false。
        /// </summary>
        [Test]
        public void HasTagExact_NotExists_ReturnsFalse()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = container.HasTagExact(TagCombat);

            Assert.That(result, Is.False);
        }

        // ============ 层级匹配测试 ============

        /// <summary>
        /// 层级匹配：容器中有 Stunned（State 的后代），查 State 应返回 true。
        /// </summary>
        [Test]
        public void HasTag_AncestorMatch_ReturnsTrue()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = container.HasTag(TagState);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// 层级匹配：容器中有 Stunned（State 分支），查 Effect 应返回 false。
        /// </summary>
        [Test]
        public void HasTag_NoMatch_ReturnsFalse()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = container.HasTag(TagEffect);

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// 层级匹配：完全相同的标签应返回 true。
        /// </summary>
        [Test]
        public void HasTag_ExactTag_ReturnsTrue()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = container.HasTag(TagStunned);

            Assert.That(result, Is.True);
        }

        // ============ 拷贝构造测试 ============

        /// <summary>
        /// 拷贝构造应复制所有标签，且克隆容器独立于源容器。
        /// </summary>
        [Test]
        public void CopyConstructor_CopiesAllTags()
        {
            var original = new GameplayTagContainer();
            original.AddTag(TagStunned);
            original.AddTag(TagEffect);

            var copy = new GameplayTagContainer(original);

            // 拷贝后内容相同
            Assert.That(copy.Count, Is.EqualTo(original.Count));
            Assert.That(copy.HasTagExact(TagStunned), Is.True);
            Assert.That(copy.HasTagExact(TagEffect), Is.True);

            // 克隆容器与源容器独立
            copy.RemoveTag(TagStunned);
            Assert.That(copy.Count, Is.EqualTo(1));
            Assert.That(original.Count, Is.EqualTo(2));
        }

        // ============ 批量匹配测试 ============

        /// <summary>
        /// HasAny：有重叠标签时应返回 true。
        /// </summary>
        [Test]
        public void HasAny_WithOverlap_ReturnsTrue()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var other = new GameplayTagContainer();
            other.AddTag(TagStunned);
            other.AddTag(TagDamage);

            var result = container.HasAny(other);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// HasAny：无重叠标签时应返回 false。
        /// </summary>
        [Test]
        public void HasAny_NoOverlap_ReturnsFalse()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var other = new GameplayTagContainer();
            other.AddTag(TagDamage);
            other.AddTag(TagBuff);

            var result = container.HasAny(other);

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// HasAll：全部存在时应返回 true。
        /// </summary>
        [Test]
        public void HasAll_AllPresent_ReturnsTrue()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var other = new GameplayTagContainer();
            other.AddTag(TagStunned);
            other.AddTag(TagCombat);

            var result = container.HasAll(other);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// HasAll：部分缺失时应返回 false。
        /// </summary>
        [Test]
        public void HasAll_SomeMissing_ReturnsFalse()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var other = new GameplayTagContainer();
            other.AddTag(TagStunned);
            other.AddTag(TagDamage);

            var result = container.HasAll(other);

            Assert.That(result, Is.False);
        }

        // ============ 批量增删测试 ============

        /// <summary>
        /// 批量添加应将另一个容器的标签合并到当前容器。
        /// </summary>
        [Test]
        public void AddTags_MergesTags()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var other = new GameplayTagContainer();
            other.AddTag(TagEffect);
            other.AddTag(TagDamage);

            container.AddTags(other);

            Assert.That(container.Count, Is.EqualTo(3));
            Assert.That(container.HasTagExact(TagStunned), Is.True);
            Assert.That(container.HasTagExact(TagEffect), Is.True);
            Assert.That(container.HasTagExact(TagDamage), Is.True);
        }

        /// <summary>
        /// 批量移除应将另一个容器的标签从当前容器中移除。
        /// </summary>
        [Test]
        public void RemoveTags_RemovesSpecified()
        {
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);
            container.AddTag(TagEffect);
            container.AddTag(TagDamage);

            var other = new GameplayTagContainer();
            other.AddTag(TagEffect);
            other.AddTag(TagDamage);

            container.RemoveTags(other);

            Assert.That(container.Count, Is.EqualTo(1));
            Assert.That(container.HasTagExact(TagStunned), Is.True);
            Assert.That(container.HasTagExact(TagEffect), Is.False);
            Assert.That(container.HasTagExact(TagDamage), Is.False);
        }
    }
}
