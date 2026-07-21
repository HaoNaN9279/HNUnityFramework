#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using HN.Framework.Core.Level.Logic.AI;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using NUnit.Framework;

    /// <summary>
    /// StrategyContext 单元测试 — 验证评估上下文对各知识池组件的聚合访问。
    /// </summary>
    [TestFixture]
    public class StrategyContextTests
    {
        private Blackboard _blackboard = null!;
        private WorldStateCache _worldState = null!;
        private PerceptionState _perception = null!;

        [SetUp]
        public void SetUp()
        {
            _blackboard = new Blackboard();
            _blackboard.Initialize();
            _worldState = new WorldStateCache();
            _worldState.Initialize();
            _perception = new PerceptionState();
            _perception.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _blackboard.Clear();
            _worldState.Clear();
            _perception.Clear();
        }

        /// <summary>
        /// 构造函数初始化后，所有聚合组件属性均不为 null。
        /// </summary>
        [Test]
        public void Constructor_InitializesAllComponents()
        {
            var context = new StrategyContext(_blackboard, _worldState, _perception);

            Assert.That(context.Blackboard, Is.Not.Null);
            Assert.That(context.WorldState, Is.Not.Null);
            Assert.That(context.Perception, Is.Not.Null);
        }

        /// <summary>
        /// 上下文应提供对 Blackboard 的正确访问。
        /// </summary>
        [Test]
        public void Context_ProvidesAccessToBlackboard()
        {
            var context = new StrategyContext(_blackboard, _worldState, _perception);

            context.Blackboard.Set("test_key", 42);

            Assert.That(context.Blackboard.Get<int>("test_key"), Is.EqualTo(42));
        }

        /// <summary>
        /// 上下文应提供对 WorldState 的正确访问。
        /// </summary>
        [Test]
        public void Context_ProvidesAccessToWorldState()
        {
            var context = new StrategyContext(_blackboard, _worldState, _perception);

            context.WorldState.Set("state", "active");

            Assert.That(context.WorldState.Get<string>("state"), Is.EqualTo("active"));
        }

        /// <summary>
        /// 上下文应提供对 PerceptionState 的正确访问。
        /// </summary>
        [Test]
        public void Context_ProvidesAccessToPerception()
        {
            var context = new StrategyContext(_blackboard, _worldState, _perception);

            context.Perception.AddVisibleTarget(1, 0.9f, 10f, new Vector3Data(0f, 0f, 0f));

            Assert.That(context.Perception.HasVisibleTargets, Is.True);
        }
    }
}
