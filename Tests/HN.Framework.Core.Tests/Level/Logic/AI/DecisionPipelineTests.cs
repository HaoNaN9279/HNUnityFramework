#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using System.Collections.Generic;
    using HN.Framework.Core.Level.Logic.AI;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using NUnit.Framework;

    /// <summary>
    /// DecisionPipeline 单元测试 — 验证管线层级设置、执行、跳过和重置的核心功能。
    /// </summary>
    [TestFixture]
    public class DecisionPipelineTests
    {
        private DecisionPipeline _pipeline = null!;
        private IEvaluationContext _testContext = null!;

        [SetUp]
        public void SetUp()
        {
            _pipeline = new DecisionPipeline();
            _pipeline.Initialize();
            _testContext = new TestEvaluationContext();
        }

        [TearDown]
        public void TearDown()
        {
            _pipeline.Clear();
        }

        /// <summary>
        /// SetLayer 配置策略后 Execute，应返回该策略 Evaluate 产生的动作指令。
        /// </summary>
        [Test]
        public void SetLayer_AndExecute_ReturnsStrategyResults()
        {
            var action = new TestActionCommand();
            var strategy = new TestDecisionStrategy("strategic_strategy")
            {
                EvaluateResult = new List<IActionCommand> { action }
            };

            _pipeline.SetLayer(PipelineLayer.Strategic, strategy);
            var result = _pipeline.Execute(_testContext);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.SameAs(action));
        }

        /// <summary>
        /// SkipLayer 后该层策略不参与执行，结果为空。
        /// </summary>
        [Test]
        public void SkipLayer_SkipsThatLayer()
        {
            var action = new TestActionCommand();
            var strategy = new TestDecisionStrategy("skipped_strategy")
            {
                EvaluateResult = new List<IActionCommand> { action }
            };

            _pipeline.SetLayer(PipelineLayer.TaskPlanning, strategy);
            _pipeline.SkipLayer(PipelineLayer.TaskPlanning);
            var result = _pipeline.Execute(_testContext);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 传入 null 上下文时 Execute 应安全返回空列表。
        /// </summary>
        [Test]
        public void Execute_NullContext_ReturnsEmpty()
        {
            var strategy = new TestDecisionStrategy("safe_strategy")
            {
                EvaluateResult = new List<IActionCommand> { new TestActionCommand() }
            };
            _pipeline.SetLayer(PipelineLayer.Strategic, strategy);

            var result = _pipeline.Execute(null!);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 有启用层时 HasAnyEnabled 返回 true。
        /// </summary>
        [Test]
        public void HasAnyEnabled_WithLayers_ReturnsTrue()
        {
            _pipeline.SetLayer(PipelineLayer.BehaviorExecution, new TestDecisionStrategy("behavior"));

            Assert.That(_pipeline.HasAnyEnabled(), Is.True);
        }

        /// <summary>
        /// 无任何启用层时 HasAnyEnabled 返回 false。
        /// </summary>
        [Test]
        public void HasAnyEnabled_NoLayers_ReturnsFalse()
        {
            Assert.That(_pipeline.HasAnyEnabled(), Is.False);
        }

        /// <summary>
        /// ResetAll 清空所有层配置，并对每个策略调用 Reset，之后无启用层。
        /// </summary>
        [Test]
        public void ResetAll_ClearsAllLayers()
        {
            var s1 = new TestDecisionStrategy("layer0");
            var s2 = new TestDecisionStrategy("layer1");
            _pipeline.SetLayer(PipelineLayer.Strategic, s1);
            _pipeline.SetLayer(PipelineLayer.TaskPlanning, s2);

            _pipeline.ResetAll();

            Assert.That(_pipeline.HasAnyEnabled(), Is.False);
            Assert.That(_pipeline.IsLayerEnabled(PipelineLayer.Strategic), Is.False);
            Assert.That(_pipeline.IsLayerEnabled(PipelineLayer.TaskPlanning), Is.False);
            Assert.That(_pipeline.GetLayer(PipelineLayer.Strategic), Is.Null);
            Assert.That(_pipeline.GetLayer(PipelineLayer.TaskPlanning), Is.Null);
            Assert.That(s1.ResetCalled, Is.True);
            Assert.That(s2.ResetCalled, Is.True);
        }

        /// <summary>
        /// 多个层配置不同策略时，Execute 累加所有层的结果。
        /// </summary>
        [Test]
        public void Execute_MultipleLayers_AccumulatesResults()
        {
            var action1 = new TestActionCommand();
            var action2 = new TestActionCommand();
            var action3 = new TestActionCommand();

            var s1 = new TestDecisionStrategy("strat1")
            {
                EvaluateResult = new List<IActionCommand> { action1 }
            };
            var s2 = new TestDecisionStrategy("strat2")
            {
                EvaluateResult = new List<IActionCommand> { action2, action3 }
            };

            _pipeline.SetLayer(PipelineLayer.Strategic, s1);
            _pipeline.SetLayer(PipelineLayer.BehaviorExecution, s2);
            var result = _pipeline.Execute(_testContext);

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0], Is.SameAs(action1));
            Assert.That(result[1], Is.SameAs(action2));
            Assert.That(result[2], Is.SameAs(action3));
        }

        /// <summary>
        /// 用于测试的 IDecisionStrategy 实现，可预设 Evaluate 返回值和追踪 Reset 调用。
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
                EvaluateResult = System.Array.Empty<IActionCommand>();
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public int Priority { get; }

            /// <inheritdoc />
            public bool IsEnabled { get; set; }

            /// <summary>
            /// 预设的 Evaluate 返回值，测试可配置。
            /// </summary>
            public IReadOnlyList<IActionCommand> EvaluateResult { get; set; }

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
                return EvaluateResult;
            }

            /// <inheritdoc />
            public void Reset()
            {
                ResetCalled = true;
            }
        }

        /// <summary>
        /// 最小化的 IActionCommand 实现，用于管线结果验证。
        /// </summary>
        private sealed class TestActionCommand : IActionCommand
        {
            /// <inheritdoc />
            public string TypeName => "TestAction";

            /// <inheritdoc />
            public int Priority { get; set; }

            /// <inheritdoc />
            public ActionCommandStatus Status { get; set; }

            /// <inheritdoc />
            public void Initialize()
            {
            }

            /// <inheritdoc />
            public void Execute()
            {
            }

            /// <inheritdoc />
            public void Cancel()
            {
            }

            /// <inheritdoc />
            public void Clear()
            {
            }
        }

        /// <summary>
        /// 最小化的 IEvaluationContext 桩，仅满足 Execute 方法的非 null 上下文要求。
        /// </summary>
        private sealed class TestEvaluationContext : IEvaluationContext
        {
            /// <inheritdoc />
            public Blackboard Blackboard => null!;

            /// <inheritdoc />
            public WorldStateCache WorldState => null!;

            /// <inheritdoc />
            public PerceptionState Perception => null!;
        }
    }
}
