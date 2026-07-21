#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using System.Collections.Generic;
    using HN.Framework.Core.Level.Logic.AI;
    using NUnit.Framework;

    /// <summary>
    /// AIAgent 单元测试 — 验证 Agent 生命周期管理、状态切换和感知-决策-执行循环。
    /// </summary>
    [TestFixture]
    public class AIAgentTests
    {
        private AIAgent _agent = null!;

        [SetUp]
        public void SetUp()
        {
            _agent = new AIAgent();
            _agent.Initialize("test_agent");
        }

        [TearDown]
        public void TearDown()
        {
            _agent.Clear();
        }

        /// <summary>
        /// Initialize 后所有子组件均非 null，Status 为 Inactive，MaxActionsPerTick 为默认值 5。
        /// </summary>
        [Test]
        public void Initialize_SetsUpAllComponents()
        {
            Assert.That(_agent.Name, Is.EqualTo("test_agent"));
            Assert.That(_agent.Status, Is.EqualTo(AgentStatus.Inactive));
            Assert.That(_agent.Blackboard, Is.Not.Null);
            Assert.That(_agent.WorldState, Is.Not.Null);
            Assert.That(_agent.Perception, Is.Not.Null);
            Assert.That(_agent.Pipeline, Is.Not.Null);
            Assert.That(_agent.Registry, Is.Not.Null);
            Assert.That(_agent.MaxActionsPerTick, Is.EqualTo(5));
        }

        /// <summary>
        /// 管线有启用层时，Activate 将 Status 切为 Active。
        /// </summary>
        [Test]
        public void Activate_ChangesStatus()
        {
            _agent.Pipeline.SetLayer(PipelineLayer.Strategic, new TestDecisionStrategy("strat"));

            _agent.Activate();

            Assert.That(_agent.Status, Is.EqualTo(AgentStatus.Active));
        }

        /// <summary>
        /// Deactivate 将 Status 切回 Inactive 并清空待执行动作队列。
        /// </summary>
        [Test]
        public void Deactivate_ClearsQueue()
        {
            var strategy = new TestDecisionStrategy("producer")
            {
                EvaluateResult = new List<IActionCommand>
                {
                    new TestActionCommand(),
                    new TestActionCommand()
                }
            };
            _agent.Pipeline.SetLayer(PipelineLayer.Strategic, strategy);
            _agent.Activate();

            // 限制每帧执行数为 0，Tick 入队但不执行
            _agent.MaxActionsPerTick = 0;
            _agent.Tick(0.1f);

            Assert.That(_agent.PendingActionCount, Is.GreaterThan(0));

            _agent.Deactivate();

            Assert.That(_agent.Status, Is.EqualTo(AgentStatus.Inactive));
            Assert.That(_agent.PendingActionCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Active 状态下 Tick 执行完整的感知→决策→执行循环，
        /// 动作指令的 Execute 被调用且队列最终为空。
        /// </summary>
        [Test]
        public void Tick_WithActive_ProcessesActions()
        {
            var action1 = new TestActionCommand();
            var action2 = new TestActionCommand();
            var strategy = new TestDecisionStrategy("executor")
            {
                EvaluateResult = new List<IActionCommand> { action1, action2 }
            };
            _agent.Pipeline.SetLayer(PipelineLayer.Strategic, strategy);
            _agent.Activate();

            _agent.Tick(0.16f);

            Assert.That(action1.ExecuteCalled, Is.True);
            Assert.That(action2.ExecuteCalled, Is.True);
            Assert.That(_agent.PendingActionCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Inactive 状态下 Tick 立即返回，不执行任何操作。
        /// </summary>
        [Test]
        public void Tick_Inactive_DoesNothing()
        {
            var action = new TestActionCommand();
            var strategy = new TestDecisionStrategy("inactive_strat")
            {
                EvaluateResult = new List<IActionCommand> { action }
            };
            _agent.Pipeline.SetLayer(PipelineLayer.Strategic, strategy);

            // 不调用 Activate，Status 保持 Inactive
            _agent.Tick(0.1f);

            Assert.That(action.ExecuteCalled, Is.False);
            Assert.That(_agent.PendingActionCount, Is.EqualTo(0));
            Assert.That(_agent.Status, Is.EqualTo(AgentStatus.Inactive));
        }

        /// <summary>
        /// Pause 将 Active 切为 Paused，Resume 将 Paused 切回 Active。
        /// </summary>
        [Test]
        public void PauseAndResume_WorkCorrectly()
        {
            _agent.Pipeline.SetLayer(PipelineLayer.Strategic, new TestDecisionStrategy("strat"));
            _agent.Activate();

            Assert.That(_agent.Status, Is.EqualTo(AgentStatus.Active));

            _agent.Pause();

            Assert.That(_agent.Status, Is.EqualTo(AgentStatus.Paused));

            _agent.Resume();

            Assert.That(_agent.Status, Is.EqualTo(AgentStatus.Active));
        }

        /// <summary>
        /// 用于测试的 IDecisionStrategy 实现，可预设 Evaluate 返回值。
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
            /// 预设的 Evaluate 返回值。
            /// </summary>
            public IReadOnlyList<IActionCommand> EvaluateResult { get; set; }

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
            }
        }

        /// <summary>
        /// 用于测试的 IActionCommand 实现，可追踪 Execute 调用。
        /// </summary>
        private sealed class TestActionCommand : IActionCommand
        {
            /// <inheritdoc />
            public string TypeName => "TestAction";

            /// <inheritdoc />
            public int Priority { get; set; }

            /// <inheritdoc />
            public ActionCommandStatus Status { get; set; }

            /// <summary>
            /// 是否已调用 Execute。
            /// </summary>
            public bool ExecuteCalled { get; private set; }

            /// <inheritdoc />
            public void Initialize()
            {
            }

            /// <inheritdoc />
            public void Execute()
            {
                ExecuteCalled = true;
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
    }
}
