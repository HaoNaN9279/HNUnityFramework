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
    /// HTNStrategy 单元测试 — 验证层次任务网络的规划器分解逻辑、
    /// 策略的整体评估流程（启停控制、规划失败处理）以及 HTNTask 的引用池生命周期。
    /// </summary>
    [TestFixture]
    public class HTNStrategyTests
    {
        private HTNStrategy _strategy = null!;
        private StrategyContext _context = null!;
        private Blackboard _blackboard = null!;
        private WorldStateCache _worldState = null!;
        private PerceptionState _perception = null!;

        /// <summary>
        /// 每个测试前初始化策略和知识池组件。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _strategy = new HTNStrategy();

            _blackboard = new Blackboard();
            _blackboard.Initialize();

            _worldState = new WorldStateCache();
            _worldState.Initialize();

            _perception = new PerceptionState();
            _perception.Initialize();

            _context = new StrategyContext(_blackboard, _worldState, _perception);
        }

        /// <summary>
        /// 每个测试后重置策略并清理知识池组件。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _strategy.Reset();
            _blackboard.Clear();
            _worldState.Clear();
            _perception.Clear();
        }

        #region 规划器测试

        /// <summary>
        /// 复合任务通过方法分解为多个原始任务后，规划器返回正确顺序的原始任务序列。
        /// 场景：Combat → MoveToEnemy, AttackEnemy。
        /// </summary>
        [Test]
        public void HTNPlanner_DecomposeCompound_PrimitiveSequence()
        {
            HTNDomain domain = new HTNDomain();

            // 注册原始任务
            HTNTask moveTask = new HTNTask
            {
                Name = "MoveToEnemy",
                Type = HTNTaskType.Primitive
            };
            HTNTask attackTask = new HTNTask
            {
                Name = "AttackEnemy",
                Type = HTNTaskType.Primitive
            };

            domain.RegisterTask(moveTask);
            domain.RegisterTask(attackTask);

            // 注册复合任务
            HTNTask combatTask = new HTNTask
            {
                Name = "Combat",
                Type = HTNTaskType.Compound
            };
            domain.RegisterTask(combatTask);

            // 为复合任务注册分解方法
            HTNMethod combatMethod = new HTNMethod
            {
                Name = "MeleeCombat",
                Subtasks = new List<HTNTask> { moveTask, attackTask }
            };
            domain.RegisterMethod("Combat", combatMethod);

            List<HTNTask> plan = HTNPlanner.Plan(domain, "Combat", _context);

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(2));
            Assert.That(plan[0].Name, Is.EqualTo("MoveToEnemy"));
            Assert.That(plan[0].Type, Is.EqualTo(HTNTaskType.Primitive));
            Assert.That(plan[1].Name, Is.EqualTo("AttackEnemy"));
            Assert.That(plan[1].Type, Is.EqualTo(HTNTaskType.Primitive));
        }

        /// <summary>
        /// 复合任务没有匹配的分解方法且无内联子任务时，规划失败返回 null。
        /// </summary>
        [Test]
        public void HTNPlanner_NoMatchingMethod_ReturnsNull()
        {
            HTNDomain domain = new HTNDomain();

            // 注册复合任务但不注册任何方法
            HTNTask combatTask = new HTNTask
            {
                Name = "Combat",
                Type = HTNTaskType.Compound
                // 不设置 Subtasks
            };
            domain.RegisterTask(combatTask);

            List<HTNTask> plan = HTNPlanner.Plan(domain, "Combat", _context);

            Assert.That(plan, Is.Null);
        }

        #endregion

        #region 策略级测试

        /// <summary>
        /// HTN 策略从复合根任务分解到原始任务后，通过工厂委托生成动作指令列表。
        /// </summary>
        [Test]
        public void HTNStrategy_Evaluate_ReturnsPrimitiveActions()
        {
            HTNDomain domain = new HTNDomain();

            // 注册原始任务（带工厂委托）
            HTNTask moveTask = new HTNTask
            {
                Name = "MoveToEnemy",
                Type = HTNTaskType.Primitive,
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    cmd.TypeName = "Wait";
                    return cmd;
                }
            };
            HTNTask attackTask = new HTNTask
            {
                Name = "AttackEnemy",
                Type = HTNTaskType.Primitive,
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    cmd.TypeName = "Attack";
                    return cmd;
                }
            };

            domain.RegisterTask(moveTask);
            domain.RegisterTask(attackTask);

            // 注册复合任务
            HTNTask combatTask = new HTNTask
            {
                Name = "Combat",
                Type = HTNTaskType.Compound
            };
            domain.RegisterTask(combatTask);

            // 为复合任务注册分解方法
            HTNMethod combatMethod = new HTNMethod
            {
                Name = "MeleeCombat",
                Subtasks = new List<HTNTask> { moveTask, attackTask }
            };
            domain.RegisterMethod("Combat", combatMethod);

            _strategy.Domain = domain;
            _strategy.RootTaskName = "Combat";
            _strategy.Initialize();

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].TypeName, Is.EqualTo("Wait"));
            Assert.That(result[1].TypeName, Is.EqualTo("Attack"));

            // 归还池化的动作指令
            foreach (IActionCommand cmd in result)
            {
                ReferencePool.Release(cmd);
            }
        }

        /// <summary>
        /// 策略禁用后 Evaluate 返回空列表，即使领域和根任务均已正确配置。
        /// </summary>
        [Test]
        public void HTNStrategy_Evaluate_Disabled_ReturnsEmpty()
        {
            HTNDomain domain = new HTNDomain();

            HTNTask primitive = new HTNTask
            {
                Name = "Move",
                Type = HTNTaskType.Primitive,
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    return cmd;
                }
            };
            domain.RegisterTask(primitive);

            HTNTask rootTask = new HTNTask
            {
                Name = "Root",
                Type = HTNTaskType.Compound
            };
            domain.RegisterTask(rootTask);

            HTNMethod method = new HTNMethod
            {
                Name = "DefaultMethod",
                Subtasks = new List<HTNTask> { primitive }
            };
            domain.RegisterMethod("Root", method);

            _strategy.Domain = domain;
            _strategy.RootTaskName = "Root";
            _strategy.Initialize();
            _strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        #endregion

        #region 引用池测试

        /// <summary>
        /// HTNTask 通过 ReferencePool 创建和释放，验证完整的池化生命周期。
        /// </summary>
        [Test]
        public void HTNTask_ReferencePool_CreateAndRelease()
        {
            HTNTask task = ReferencePool.Acquire<HTNTask>();

            Assert.That(task, Is.Not.Null);
            Assert.That(task.Name, Is.EqualTo(string.Empty));
            Assert.That(task.Type, Is.EqualTo(HTNTaskType.Primitive));
            Assert.That(task.ActionFactory, Is.Null);
            Assert.That(task.Condition, Is.Null);
            Assert.That(task.Subtasks, Is.Null);

            // 设置属性
            task.Name = "TestTask";
            task.Type = HTNTaskType.Compound;
            task.Condition = ctx => true;

            Assert.That(task.Name, Is.EqualTo("TestTask"));
            Assert.That(task.Type, Is.EqualTo(HTNTaskType.Compound));

            ReferencePool.Release(task);

            // 从池中重新获取，应为清除后状态
            HTNTask task2 = ReferencePool.Acquire<HTNTask>();
            Assert.That(task2.Name, Is.EqualTo(string.Empty));
            Assert.That(task2.Type, Is.EqualTo(HTNTaskType.Primitive));
            Assert.That(task2.Condition, Is.Null);

            ReferencePool.Release(task2);
        }

        #endregion

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
