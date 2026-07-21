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
    /// DecisionTreeStrategy 单元测试 — 验证决策树策略的节点构建、分支评估、启停控制和对象池生命周期。
    /// </summary>
    [TestFixture]
    public class DecisionTreeStrategyTests
    {
        private DecisionTreeStrategy _strategy = null!;
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
            _strategy = new DecisionTreeStrategy();
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
        /// 每个测试后清理策略和上下文，释放引用池对象。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _strategy.Reset();
            _blackboard.Clear();
            _worldState.Clear();
            _perception.Clear();
        }

        /// <summary>
        /// 分别添加条件节点和动作节点后，验证节点名称、类型和非空。
        /// </summary>
        [Test]
        public void AddConditionNode_AndAddActionNode_BuildsTree()
        {
            DecisionTreeNode cond = _strategy.AddConditionNode("IsReady", ctx => true);
            DecisionTreeNode action = _strategy.AddActionNode("Idle", ctx => ReferencePool.Acquire<TestActionCommand>());

            Assert.That(cond, Is.Not.Null);
            Assert.That(cond.Name, Is.EqualTo("IsReady"));
            Assert.That(cond.NodeType, Is.EqualTo(DecisionTreeNodeType.Condition));

            Assert.That(action, Is.Not.Null);
            Assert.That(action.Name, Is.EqualTo("Idle"));
            Assert.That(action.NodeType, Is.EqualTo(DecisionTreeNodeType.Action));
        }

        /// <summary>
        /// 设置根节点后执行评估，验证通过 TrueNode 分支正确收集动作指令。
        /// </summary>
        [Test]
        public void SetRoot_AndEvaluate_ReturnsCorrectAction()
        {
            DecisionTreeNode actionNode = _strategy.AddActionNode("Patrol", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Wait";
                return cmd;
            });
            DecisionTreeNode root = _strategy.AddConditionNode("Root", ctx => true);
            root.TrueNode = actionNode;
            _strategy.SetRoot(root);

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.Not.Null);
            Assert.That(result[0].TypeName, Is.EqualTo("Wait"));

            // 释放池化的动作指令
            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 嵌套条件树中，条件为 true 时沿 TrueNode 分支到达动作节点。
        /// </summary>
        [Test]
        public void Evaluate_WithNestedConditions_FollowsTrueBranch()
        {
            DecisionTreeNode innerAction = _strategy.AddActionNode("Attack", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Attack";
                return cmd;
            });
            DecisionTreeNode innerCond = _strategy.AddConditionNode("HasAmmo", ctx => true);
            innerCond.TrueNode = innerAction;

            DecisionTreeNode root = _strategy.AddConditionNode("EnemyNear", ctx => true);
            root.TrueNode = innerCond;
            _strategy.SetRoot(root);

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Attack"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 嵌套条件树中，条件为 false 时沿 FalseNode 分支到达备选动作节点。
        /// </summary>
        [Test]
        public void Evaluate_WithNestedConditions_FollowsFalseBranch()
        {
            DecisionTreeNode idleAction = _strategy.AddActionNode("Idle", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Wait";
                return cmd;
            });
            DecisionTreeNode attackAction = _strategy.AddActionNode("Attack", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Attack";
                return cmd;
            });

            DecisionTreeNode innerCond = _strategy.AddConditionNode("HasAmmo", ctx => false);
            innerCond.TrueNode = attackAction;
            innerCond.FalseNode = idleAction;

            DecisionTreeNode root = _strategy.AddConditionNode("EnemyNear", ctx => true);
            root.TrueNode = innerCond;
            _strategy.SetRoot(root);

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Wait"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 策略禁用后，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void Evaluate_DisabledStrategy_ReturnsEmpty()
        {
            DecisionTreeNode actionNode = _strategy.AddActionNode("Do", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            });
            DecisionTreeNode root = _strategy.AddConditionNode("Root", ctx => true);
            root.TrueNode = actionNode;
            _strategy.SetRoot(root);
            _strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 上下文为 null 时，由于条件委托检查 null 返回 false，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void Evaluate_NullContext_ReturnsEmpty()
        {
            DecisionTreeNode actionNode = _strategy.AddActionNode("Do", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            });
            DecisionTreeNode root = _strategy.AddConditionNode("Root", ctx => ctx != null);
            root.TrueNode = actionNode;
            _strategy.SetRoot(root);

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(null!);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Reset 后所有节点归还引用池，内部状态清空。
        /// </summary>
        [Test]
        public void Reset_ClearsAllNodes()
        {
            _strategy.AddConditionNode("C1", ctx => true);
            _strategy.AddActionNode("A1", ctx => null);
            _strategy.SetRoot(_strategy.AddConditionNode("Root", ctx => true));

            _strategy.Reset();

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// DecisionTreeNode 通过 ReferencePool 创建和释放，验证完整的池化生命周期。
        /// </summary>
        [Test]
        public void DecisionTreeNode_ReferencePool_CreateAndRelease()
        {
            DecisionTreeNode node = ReferencePool.Acquire<DecisionTreeNode>();

            Assert.That(node, Is.Not.Null);
            Assert.That(node.Name, Is.Null);
            Assert.That(node.NodeType, Is.EqualTo(default(DecisionTreeNodeType)));

            node.Name = "TestNode";
            node.NodeType = DecisionTreeNodeType.Action;
            Assert.That(node.Name, Is.EqualTo("TestNode"));

            ReferencePool.Release(node);

            DecisionTreeNode node2 = ReferencePool.Acquire<DecisionTreeNode>();
            // 从池中获取的节点应为清除后状态
            Assert.That(node2.Name, Is.Null);
            ReferencePool.Release(node2);
        }

        /// <summary>
        /// 测试用 ActionCommand — 实现 IActionCommand 和 IReference 的最小 mock。
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
