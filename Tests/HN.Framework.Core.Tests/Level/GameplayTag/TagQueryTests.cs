#nullable enable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using GameplayTag = HN.Framework.Core.Level.GameplayTag;
using GameplayTagManager = HN.Framework.Core.Level.GameplayTagManager;
using GameplayTagContainer = HN.Framework.Core.Level.GameplayTagContainer;
using TagQuery = HN.Framework.Core.Level.TagQuery;
using GameplayTagDefinition = HN.Framework.Core.Level.GameplayTagManager.GameplayTagDefinition;

namespace HN.Framework.Core.Tests.Level.GameplayTagTests
{
    /// <summary>
    /// <see cref="TagQuery"/> 的单元测试。
    /// 测试 TagQuery 静态工厂方法及表达式树评估逻辑。
    /// 覆盖 Make、Any、All、Not 单节点及嵌套组合查询。
    /// </summary>
    [TestFixture]
    public class TagQueryTests
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

        // ============ 空查询测试 ============

        /// <summary>
        /// 空查询（无任何条件）应对任意容器返回 true。
        /// </summary>
        [Test]
        public void EmptyQuery_AlwaysMatches()
        {
            var query = TagQuery.Any(null!);
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            Assert.That(query.IsEmpty, Is.True);
            Assert.That(query.Matches(container), Is.True);
        }

        // ============ Make 单标签查询测试 ============

        /// <summary>
        /// Make(tag) 创建的查询：容器包含该标签时应匹配。
        /// </summary>
        [Test]
        public void Make_WithTag_MatchesContainerHavingThatTag()
        {
            var query = TagQuery.Make(TagStunned);
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = query.Matches(container);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Make(tag) 创建的查询：容器不包含该标签时应不匹配。
        /// </summary>
        [Test]
        public void Make_WithTag_NotMatchesContainerWithoutTag()
        {
            var query = TagQuery.Make(TagStunned);
            var container = new GameplayTagContainer();
            container.AddTag(TagEffect);

            var result = query.Matches(container);

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Make(tag) 使用层级匹配：容器中有 Stunned 时，
        /// Make(State) 应通过层级匹配返回 true。
        /// </summary>
        [Test]
        public void Make_AncestorTag_MatchesDescendantInContainer()
        {
            var query = TagQuery.Make(TagState);
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = query.Matches(container);

            Assert.That(result, Is.True);
        }

        // ============ Any 组合查询测试 ============

        /// <summary>
        /// Any(A, B, C) 查询：容器匹配其中一个子查询时应返回 true。
        /// </summary>
        [Test]
        public void Any_OneChildMatch_ReturnsTrue()
        {
            var query = TagQuery.Any(
                TagQuery.Make(TagStunned),
                TagQuery.Make(TagCombat),
                TagQuery.Make(TagEffect)
            );
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = query.Matches(container);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Any(A, B) 查询：容器不匹配任何子查询时应返回 false。
        /// </summary>
        [Test]
        public void Any_NoMatch_ReturnsFalse()
        {
            var query = TagQuery.Any(
                TagQuery.Make(TagStunned),
                TagQuery.Make(TagCombat)
            );
            var container = new GameplayTagContainer();
            container.AddTag(TagEffect);

            var result = query.Matches(container);

            Assert.That(result, Is.False);
        }

        // ============ All 组合查询测试 ============

        /// <summary>
        /// All(A, B) 查询：容器满足所有子查询时应返回 true。
        /// 容器中有 Stunned → 层级匹配 Stunned 和 Combat 均满足。
        /// </summary>
        [Test]
        public void All_AllMatch_ReturnsTrue()
        {
            var query = TagQuery.All(
                TagQuery.Make(TagStunned),
                TagQuery.Make(TagCombat)
            );
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = query.Matches(container);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// All(A, B) 查询：容器满足 A 但不满足 B 时应返回 false。
        /// </summary>
        [Test]
        public void All_OneMissing_ReturnsFalse()
        {
            var query = TagQuery.All(
                TagQuery.Make(TagStunned),
                TagQuery.Make(TagDamage)
            );
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = query.Matches(container);

            Assert.That(result, Is.False);
        }

        // ============ Not 查询测试 ============

        /// <summary>
        /// Not(A) 查询：容器包含 A 时应返回 false。
        /// </summary>
        [Test]
        public void Not_ContainerHasTag_ReturnsFalse()
        {
            var query = TagQuery.Not(TagQuery.Make(TagStunned));
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = query.Matches(container);

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Not(A) 查询：容器不包含 A 时应返回 true。
        /// </summary>
        [Test]
        public void Not_ContainerWithoutTag_ReturnsTrue()
        {
            var query = TagQuery.Not(TagQuery.Make(TagDamage));
            var container = new GameplayTagContainer();
            container.AddTag(TagStunned);

            var result = query.Matches(container);

            Assert.That(result, Is.True);
        }

        // ============ 嵌套组合查询测试 ============

        /// <summary>
        /// 复杂嵌套查询 Any(All(Stunned, Combat), Not(Damage))：
        /// - 容器有 Stunned → All 分支匹配，Any 短路返回 true。
        /// - 容器有 Effect → All 不匹配，Not 分支匹配（无 Damage），Any 返回 true。
        /// - 容器有 Damage → 两个分支均不匹配，Any 返回 false。
        /// </summary>
        [Test]
        public void NestedQuery_ComplexExpression()
        {
            var query = TagQuery.Any(
                TagQuery.All(
                    TagQuery.Make(TagStunned),
                    TagQuery.Make(TagCombat)
                ),
                TagQuery.Not(TagQuery.Make(TagDamage))
            );

            // 容器有 Stunned → All 匹配（层级），Any 短路为 true
            var stunnedContainer = new GameplayTagContainer();
            stunnedContainer.AddTag(TagStunned);
            Assert.That(query.Matches(stunnedContainer), Is.True, "Should match via All branch");

            // 容器有 Effect（无 Damage）→ All 不匹配，Not(Damage) 匹配
            var effectContainer = new GameplayTagContainer();
            effectContainer.AddTag(TagEffect);
            Assert.That(query.Matches(effectContainer), Is.True, "Should match via Not branch");

            // 容器有 Damage → All 不匹配，Not(Damage) 也不匹配（因为有 Damage）
            var damageContainer = new GameplayTagContainer();
            damageContainer.AddTag(TagDamage);
            Assert.That(query.Matches(damageContainer), Is.False, "Should not match: All fails, Not fails");
        }

        // ============ 边界条件测试 ============

        /// <summary>
        /// Any(null) 应返回空查询（IsEmpty=true，始终匹配）。
        /// </summary>
        [Test]
        public void Any_NullOrEmpty_ReturnsEmptyQuery()
        {
            var query = TagQuery.Any(null!);

            Assert.That(query.IsEmpty, Is.True);
        }

        /// <summary>
        /// Any() 无参数应返回空查询。
        /// </summary>
        [Test]
        public void Any_NoArguments_ReturnsEmptyQuery()
        {
            var query = TagQuery.Any();

            Assert.That(query.IsEmpty, Is.True);
        }

        /// <summary>
        /// All(null) 应返回空查询。
        /// </summary>
        [Test]
        public void All_NullOrEmpty_ReturnsEmptyQuery()
        {
            var query = TagQuery.All(null!);

            Assert.That(query.IsEmpty, Is.True);
        }

        /// <summary>
        /// Not(null) 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void Not_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => TagQuery.Not(null!));
        }

        /// <summary>
        /// ToString 应返回非空字符串表示。
        /// </summary>
        [Test]
        public void ToString_NonEmpty_ReturnsString()
        {
            var query = TagQuery.Any(
                TagQuery.Make(TagStunned),
                TagQuery.Not(TagQuery.Make(TagDamage))
            );

            var str = query.ToString();

            Assert.That(str, Is.Not.Null);
            Assert.That(str.Length, Is.GreaterThan(0));
        }
    }
}
