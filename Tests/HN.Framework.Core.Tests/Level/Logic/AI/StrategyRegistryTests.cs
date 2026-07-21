#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using System.Collections.Generic;
    using HN.Framework.Core.Level.Logic.AI;
    using NUnit.Framework;

    /// <summary>
    /// StrategyRegistry 单元测试 — 验证策略注册、查询、注销和生命周期管理的核心功能。
    /// </summary>
    [TestFixture]
    public class StrategyRegistryTests
    {
        private StrategyRegistry _registry = null!;

        [SetUp]
        public void SetUp()
        {
            _registry = new StrategyRegistry();
            _registry.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _registry.Clear();
        }

        /// <summary>
        /// 注册策略后，通过 Get 按名称获取应返回同一实例。
        /// </summary>
        [Test]
        public void Register_AndGet_ReturnsStrategy()
        {
            var strategy = new TestDecisionStrategy("combat");

            _registry.Register(strategy);
            var result = _registry.Get("combat");

            Assert.That(result, Is.SameAs(strategy));
        }

        /// <summary>
        /// 以相同名称注册新策略时，应覆盖旧策略。
        /// </summary>
        [Test]
        public void Register_Overwrite_ReplacesExisting()
        {
            var first = new TestDecisionStrategy("utility");
            var second = new TestDecisionStrategy("utility");

            _registry.Register(first);
            _registry.Register(second);
            var result = _registry.Get("utility");

            Assert.That(result, Is.SameAs(second));
            Assert.That(result, Is.Not.SameAs(first));
        }

        /// <summary>
        /// 注销已注册策略后，Get 返回 null 且 Has 返回 false。
        /// </summary>
        [Test]
        public void Unregister_RemovesStrategy()
        {
            var strategy = new TestDecisionStrategy("patrol");
            _registry.Register(strategy);

            _registry.Unregister("patrol");

            Assert.That(_registry.Get("patrol"), Is.Null);
            Assert.That(_registry.Has("patrol"), Is.False);
        }

        /// <summary>
        /// 已注册的策略键，Has 应返回 true。
        /// </summary>
        [Test]
        public void Has_ExistingKey_ReturnsTrue()
        {
            _registry.Register(new TestDecisionStrategy("flee"));

            Assert.That(_registry.Has("flee"), Is.True);
        }

        /// <summary>
        /// 未注册的策略键，Has 应返回 false。
        /// </summary>
        [Test]
        public void Has_NonExistentKey_ReturnsFalse()
        {
            Assert.That(_registry.Has("nonexistent"), Is.False);
        }

        /// <summary>
        /// GetAll 返回所有已注册策略的只读集合，数量与注册数一致。
        /// </summary>
        [Test]
        public void GetAll_ReturnsAllRegistered()
        {
            var s1 = new TestDecisionStrategy("a");
            var s2 = new TestDecisionStrategy("b");
            var s3 = new TestDecisionStrategy("c");
            _registry.Register(s1);
            _registry.Register(s2);
            _registry.Register(s3);

            var all = _registry.GetAll();

            Assert.That(all.Count, Is.EqualTo(3));
            Assert.That(all, Does.Contain(s1));
            Assert.That(all, Does.Contain(s2));
            Assert.That(all, Does.Contain(s3));
        }

        /// <summary>
        /// UnregisterAll 清空所有已注册策略，并对每个策略调用 Reset。
        /// </summary>
        [Test]
        public void UnregisterAll_ClearsAll()
        {
            var s1 = new TestDecisionStrategy("x");
            var s2 = new TestDecisionStrategy("y");
            _registry.Register(s1);
            _registry.Register(s2);

            _registry.UnregisterAll();

            Assert.That(_registry.Count, Is.EqualTo(0));
            Assert.That(_registry.Has("x"), Is.False);
            Assert.That(_registry.Has("y"), Is.False);
            Assert.That(s1.ResetCalled, Is.True);
            Assert.That(s2.ResetCalled, Is.True);
        }

        /// <summary>
        /// Clear 释放内部字典到引用池，之后操作均为空操作不抛异常，
        /// 且重新 Initialize 后可正常使用。
        /// </summary>
        [Test]
        public void Clear_ReleasesDictionary()
        {
            var strategy = new TestDecisionStrategy("target");
            _registry.Register(strategy);

            _registry.Clear();

            // Clear 后所有查询操作均为空操作
            Assert.That(_registry.Count, Is.EqualTo(0));
            Assert.That(_registry.Has("target"), Is.False);
            Assert.That(_registry.Get("target"), Is.Null);
            Assert.That(_registry.GetAll().Count, Is.EqualTo(0));
            Assert.That(strategy.ResetCalled, Is.True);

            // 重新初始化后可正常使用
            _registry.Initialize();
            _registry.Register(new TestDecisionStrategy("new"));

            Assert.That(_registry.Count, Is.EqualTo(1));
            Assert.That(_registry.Has("new"), Is.True);

            // 手动 Clear 释放重新初始化的字典
            _registry.Clear();
        }

        /// <summary>
        /// 用于测试的 IDecisionStrategy 实现，可追踪 Reset 调用。
        /// </summary>
        private sealed class TestDecisionStrategy : IDecisionStrategy
        {
            /// <summary>
            /// 初始化测试策略。
            /// </summary>
            /// <param name="name">策略名称。</param>
            /// <param name="priority">优先级，默认为 0。</param>
            public TestDecisionStrategy(string name, int priority = 0)
            {
                Name = name;
                Priority = priority;
                IsEnabled = true;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public int Priority { get; }

            /// <inheritdoc />
            public bool IsEnabled { get; set; }

            /// <summary>
            /// 是否已调用 Reset。
            /// </summary>
            public bool ResetCalled { get; private set; }

            /// <inheritdoc />
            public void Initialize()
            {
            }

            /// <inheritdoc />
            public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
            {
                return System.Array.Empty<IActionCommand>();
            }

            /// <inheritdoc />
            public void Reset()
            {
                ResetCalled = true;
            }
        }
    }
}
