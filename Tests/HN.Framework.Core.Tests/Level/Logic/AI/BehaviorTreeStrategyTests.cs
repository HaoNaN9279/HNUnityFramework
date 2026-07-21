#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
    using HN.Framework.Core.Level.Logic.AI;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using HN.Framework.Core.Level.Logic.AI.Strategies;
    using NUnit.Framework;

    /// <summary>
    /// 评估上下文桩 — 为没有特殊数据需求的节点测试提供最小化上下文。
    /// </summary>
    internal sealed class StubEvaluationContext : IEvaluationContext
    {
        public Blackboard Blackboard { get; } = null!;
        public WorldStateCache WorldState { get; } = null!;
        public PerceptionState Perception { get; } = null!;
    }

    /// <summary>
    /// BehaviorTreeStrategy 单元测试 — 验证行为树各类型节点的评估逻辑、
    /// 策略整体行为（启停控制、根节点为空）以及引用池生命周期。
    /// </summary>
    [TestFixture]
    public class BehaviorTreeStrategyTests
    {
        private BehaviorTreeStrategy _strategy = null!;
        private StrategyContext _context = null!;
        private Blackboard _blackboard = null!;
        private WorldStateCache _worldState = null!;
        private PerceptionState _perception = null!;

        /// <summary>
        /// 每个测试前初始化策略和评估上下文。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _strategy = new BehaviorTreeStrategy();
            _strategy.Initialize();

            _blackboard = new Blackboard();
            _blackboard.Initialize();

            _worldState = new WorldStateCache();
            _worldState.Initialize();

            _perception = new PerceptionState();
            _perception.Initialize();

            _context = new StrategyContext(_blackboard, _worldState, _perception);
        }

        /// <summary>
        /// 每个测试后重置策略并清理知识池组件，归还引用池对象。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _strategy.Reset();
            _blackboard.Clear();
            _worldState.Clear();
            _perception.Clear();
        }

        #region 节点级测试

        /// <summary>
        /// 顺序节点包含两个全部成功的动作节点时，返回 Success。
        /// </summary>
        [Test]
        public void SequenceNode_AllSuccess_ReturnsSuccess()
        {
            SequenceNode sequence = ReferencePool.Acquire<SequenceNode>();
            sequence.Name = "TestSequence";

            ActionNode action1 = CreateSuccessAction("Action1");
            ActionNode action2 = CreateSuccessAction("Action2");
            sequence.AddChild(action1);
            sequence.AddChild(action2);

            IEvaluationContext stubContext = new StubEvaluationContext();
            BTNodeStatus status = sequence.OnEvaluate(stubContext);

            Assert.That(status, Is.EqualTo(BTNodeStatus.Success));
            Assert.That(sequence.Status, Is.EqualTo(BTNodeStatus.Success));

            // 确保子节点被执行：两个动作节点都产生了指令
            Assert.That(action1.CurrentAction, Is.Not.Null);
            Assert.That(action2.CurrentAction, Is.Not.Null);

            // 归还引用池
            ReferencePool.Release(sequence);
            ReferencePool.Release(action1.CurrentAction!);
            ReferencePool.Release(action2.CurrentAction!);
            ReferencePool.Release(action1);
            ReferencePool.Release(action2);
        }

        /// <summary>
        /// 选择节点的第一个子节点返回 Success 时，整体返回 Success 且不评估后续子节点。
        /// </summary>
        [Test]
        public void SelectorNode_FirstSuccess_ReturnsSuccess()
        {
            SelectorNode selector = ReferencePool.Acquire<SelectorNode>();
            selector.Name = "TestSelector";

            ActionNode action1 = CreateSuccessAction("Action1");
            ActionNode action2 = CreateFailureAction("Action2");
            selector.AddChild(action1);
            selector.AddChild(action2);

            IEvaluationContext stubContext = new StubEvaluationContext();
            BTNodeStatus status = selector.OnEvaluate(stubContext);

            Assert.That(status, Is.EqualTo(BTNodeStatus.Success));
            Assert.That(selector.CurrentAction, Is.Not.Null);

            ReferencePool.Release(selector);
            if (action1.CurrentAction != null) ReferencePool.Release(action1.CurrentAction);
            if (action2.CurrentAction != null) ReferencePool.Release(action2.CurrentAction);
            ReferencePool.Release(action1);
            ReferencePool.Release(action2);
        }

        /// <summary>
        /// 反转节点将子节点的 Success 反转为 Failure，Failure 反转为 Success。
        /// </summary>
        [Test]
        public void InverterNode_InvertsResult()
        {
            IEvaluationContext stubContext = new StubEvaluationContext();

            // Success → Failure
            InverterNode inverter1 = ReferencePool.Acquire<InverterNode>();
            ActionNode successAction = CreateSuccessAction("SuccessAction");
            inverter1.Child = successAction;

            BTNodeStatus status1 = inverter1.OnEvaluate(stubContext);
            Assert.That(status1, Is.EqualTo(BTNodeStatus.Failure));

            ReferencePool.Release(inverter1);
            if (successAction.CurrentAction != null) ReferencePool.Release(successAction.CurrentAction);
            ReferencePool.Release(successAction);

            // Failure → Success
            InverterNode inverter2 = ReferencePool.Acquire<InverterNode>();
            ActionNode failureAction = CreateFailureAction("FailureAction");
            inverter2.Child = failureAction;

            BTNodeStatus status2 = inverter2.OnEvaluate(stubContext);
            Assert.That(status2, Is.EqualTo(BTNodeStatus.Success));

            ReferencePool.Release(inverter2);
            if (failureAction.CurrentAction != null) ReferencePool.Release(failureAction.CurrentAction);
            ReferencePool.Release(failureAction);
        }

        /// <summary>
        /// 动作节点的工厂委托返回有效指令时，节点返回 Success 并持有该指令。
        /// </summary>
        [Test]
        public void ActionNode_WithFactory_ReturnsActionCommand()
        {
            ActionNode action = ReferencePool.Acquire<ActionNode>();
            action.Name = "TestAction";
            action.ActionFactory = ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            };

            IEvaluationContext stubContext = new StubEvaluationContext();
            BTNodeStatus status = action.OnEvaluate(stubContext);

            Assert.That(status, Is.EqualTo(BTNodeStatus.Success));
            Assert.That(action.CurrentAction, Is.Not.Null);
            Assert.That(action.CurrentAction.TypeName, Is.EqualTo("TestAction"));

            ReferencePool.Release(action.CurrentAction);
            ReferencePool.Release(action);
        }

        /// <summary>
        /// 并行节点中达到成功阈值时返回 Success。
        /// </summary>
        [Test]
        public void ParallelNode_ThresholdMet_ReturnsSuccess()
        {
            ParallelNode parallel = ReferencePool.Acquire<ParallelNode>();
            parallel.Name = "TestParallel";
            parallel.SuccessThreshold = 2;

            ActionNode success1 = CreateSuccessAction("S1");
            ActionNode success2 = CreateSuccessAction("S2");
            ActionNode failure = CreateFailureAction("F1");
            parallel.AddChild(success1);
            parallel.AddChild(success2);
            parallel.AddChild(failure);

            IEvaluationContext stubContext = new StubEvaluationContext();
            BTNodeStatus status = parallel.OnEvaluate(stubContext);

            Assert.That(status, Is.EqualTo(BTNodeStatus.Success));

            ReferencePool.Release(parallel);
            if (success1.CurrentAction != null) ReferencePool.Release(success1.CurrentAction);
            if (success2.CurrentAction != null) ReferencePool.Release(success2.CurrentAction);
            if (failure.CurrentAction != null) ReferencePool.Release(failure.CurrentAction);
            ReferencePool.Release(success1);
            ReferencePool.Release(success2);
            ReferencePool.Release(failure);
        }

        /// <summary>
        /// 重复节点设置 RepeatCount=3 时，子节点返回 Success 后继续重复，
        /// 前两次返回 Running，第三次返回 Success。
        /// </summary>
        [Test]
        public void RepeaterNode_RepeatsSpecifiedCount()
        {
            RepeaterNode repeater = ReferencePool.Acquire<RepeaterNode>();
            repeater.Name = "TestRepeater";
            repeater.RepeatCount = 3;

            ActionNode child = CreateSuccessAction("RepeatAction");
            repeater.Child = child;

            IEvaluationContext stubContext = new StubEvaluationContext();

            // 第 1 次：CurrentCount=0 < 3，子节点 Success → CurrentCount=1，返回 Running
            BTNodeStatus status1 = repeater.OnEvaluate(stubContext);
            Assert.That(status1, Is.EqualTo(BTNodeStatus.Running));
            Assert.That(repeater.CurrentCount, Is.EqualTo(1));

            // 第 2 次：CurrentCount=1 < 3，子节点 Success → CurrentCount=2，返回 Running
            child.CurrentAction = null; // 模拟新帧
            BTNodeStatus status2 = repeater.OnEvaluate(stubContext);
            Assert.That(status2, Is.EqualTo(BTNodeStatus.Running));
            Assert.That(repeater.CurrentCount, Is.EqualTo(2));

            // 第 3 次：CurrentCount=2 < 3，子节点 Success → CurrentCount=3 >= 3，返回 Success
            child.CurrentAction = null;
            BTNodeStatus status3 = repeater.OnEvaluate(stubContext);
            Assert.That(status3, Is.EqualTo(BTNodeStatus.Success));
            Assert.That(repeater.CurrentCount, Is.EqualTo(3));

            ReferencePool.Release(repeater);
            if (child.CurrentAction != null) ReferencePool.Release(child.CurrentAction);
            ReferencePool.Release(child);
        }

        /// <summary>
        /// 条件装饰节点的条件不满足时，即使子节点会成功，也返回 Failure。
        /// </summary>
        [Test]
        public void ConditionalDecorator_ConditionFalse_Fails()
        {
            ConditionalDecorator conditional = ReferencePool.Acquire<ConditionalDecorator>();
            conditional.Name = "TestConditional";
            conditional.Condition = ctx => false;

            ActionNode child = CreateSuccessAction("ChildAction");
            conditional.Child = child;

            IEvaluationContext stubContext = new StubEvaluationContext();
            BTNodeStatus status = conditional.OnEvaluate(stubContext);

            Assert.That(status, Is.EqualTo(BTNodeStatus.Failure));
            Assert.That(conditional.CurrentAction, Is.Null);

            ReferencePool.Release(conditional);
            if (child.CurrentAction != null) ReferencePool.Release(child.CurrentAction);
            ReferencePool.Release(child);
        }

        #endregion

        #region 策略级测试

        /// <summary>
        /// 策略设置根节点后评估，应返回由 ActionNode 产生的动作指令。
        /// </summary>
        [Test]
        public void BehaviorTreeStrategy_Evaluate_WithRoot_ReturnsActions()
        {
            SequenceNode root = _strategy.AddSequence("Root");
            ActionNode action = _strategy.AddAction("Attack", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Attack";
                return cmd;
            });
            root.AddChild(action);
            _strategy.SetRoot(root);

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Attack"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 策略禁用后，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void BehaviorTreeStrategy_Evaluate_Disabled_ReturnsEmpty()
        {
            ActionNode action = _strategy.AddAction("Do", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            });
            SequenceNode root = _strategy.AddSequence("Root");
            root.AddChild(action);
            _strategy.SetRoot(root);
            _strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 未设置根节点时，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void BehaviorTreeStrategy_Evaluate_NullRoot_ReturnsEmpty()
        {
            // 添加节点但不设置根
            _strategy.AddAction("Idle", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            });

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建一个始终返回 Success 的动作节点。
        /// </summary>
        private static ActionNode CreateSuccessAction(string name)
        {
            ActionNode node = ReferencePool.Acquire<ActionNode>();
            node.Name = name;
            node.ActionFactory = ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            };

            return node;
        }

        /// <summary>
        /// 创建一个始终返回 Failure 的动作节点（工厂委托返回 null）。
        /// </summary>
        private static ActionNode CreateFailureAction(string name)
        {
            ActionNode node = ReferencePool.Acquire<ActionNode>();
            node.Name = name;
            node.ActionFactory = ctx => null!;

            return node;
        }

        #endregion

        /// <summary>
        /// 测试用 ActionCommand — 实现 IActionCommand 和 IReference 的最小 mock，
        /// 替代已删除的预设 Action 类型（WaitAction、AttackAction 等）。
        /// </summary>
        private sealed class TestActionCommand : IReference, IActionCommand
        {
            public string TypeName { get; set; } = "TestAction";
            public int Priority { get; set; }
            public ActionCommandStatus Status { get; set; }

            public void Initialize()
            {
                Status = ActionCommandStatus.Pending;
            }

            public void Execute()
            {
                Status = ActionCommandStatus.Running;
            }

            public void Cancel()
            {
                Status = ActionCommandStatus.Cancelled;
            }

            public void Clear()
            {
                TypeName = "TestAction";
                Priority = 0;
                Status = ActionCommandStatus.Pending;
            }
        }
    }
}
