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
    /// GOAPStrategy 单元测试 — 验证 GOAP 世界状态操作、
    /// A* 规划器的搜索正确性以及策略的整体评估流程（启停控制、目标/动作管理）。
    /// </summary>
    [TestFixture]
    public class GOAPStrategyTests
    {
        private GOAPStrategy _strategy = null!;
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
            _strategy = new GOAPStrategy();

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

        #region 世界状态测试

        /// <summary>
        /// 克隆后的世界状态与原状态独立：修改原状态不影响克隆副本。
        /// </summary>
        [Test]
        public void GOAPWorldState_Clone_IndependentCopy()
        {
            GOAPWorldState original = new GOAPWorldState(4);
            original.Set("HasWeapon", true);
            original.Set("IsEnemyDead", false);

            GOAPWorldState clone = original.Clone();

            // 克隆副本应有相同的值
            Assert.That(clone.Get("HasWeapon"), Is.True);
            Assert.That(clone.Get("IsEnemyDead"), Is.False);

            // 修改原状态
            original.Set("HasWeapon", false);
            original.Set("HasAmmo", true);

            // 克隆副本不受影响
            Assert.That(clone.Get("HasWeapon"), Is.True);
            Assert.That(clone.Get("HasAmmo"), Is.False);
        }

        /// <summary>
        /// ApplyEffects 将效果字典的键值对原地写入世界状态。
        /// </summary>
        [Test]
        public void GOAPWorldState_ApplyEffects_UpdatesState()
        {
            GOAPWorldState state = new GOAPWorldState(4);
            state.Set("HasWeapon", true);

            Dictionary<string, bool> effects = new Dictionary<string, bool>
            {
                { "IsEnemyDead", true },
                { "HasWeapon", false }
            };

            state.ApplyEffects(effects);

            Assert.That(state.Get("HasWeapon"), Is.False);
            Assert.That(state.Get("IsEnemyDead"), Is.True);
        }

        #endregion

        #region A* 规划器测试

        /// <summary>
        /// A* 搜索在存在有效动作链时，返回从起始状态到达目标状态的动作序列。
        /// 场景：EquipWeapon → Attack 完成 KillEnemy 目标。
        /// </summary>
        [Test]
        public void GOAPPlanner_AStarSearch_FindsValidPlan()
        {
            GOAPWorldState startState = new GOAPWorldState(4);
            startState.Set("HasWeaponInInventory", true);

            GOAPGoal goal = new GOAPGoal
            {
                Name = "KillEnemy",
                Priority = 10,
                GoalConditions = new Dictionary<string, bool>
                {
                    { "IsEnemyDead", true }
                }
            };

            GOAPAction equipWeapon = new GOAPAction
            {
                Name = "EquipWeapon",
                Cost = 1f,
                Preconditions = new Dictionary<string, bool>
                {
                    { "HasWeaponInInventory", true }
                },
                Effects = new Dictionary<string, bool>
                {
                    { "HasWeapon", true }
                }
            };

            GOAPAction attack = new GOAPAction
            {
                Name = "Attack",
                Cost = 2f,
                Preconditions = new Dictionary<string, bool>
                {
                    { "HasWeapon", true }
                },
                Effects = new Dictionary<string, bool>
                {
                    { "IsEnemyDead", true }
                }
            };

            List<GOAPAction> availableActions = new List<GOAPAction> { equipWeapon, attack };

            List<GOAPAction> plan = GOAPPlanner.AStarSearch(startState, goal, availableActions);

            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(2));
            Assert.That(plan[0].Name, Is.EqualTo("EquipWeapon"));
            Assert.That(plan[1].Name, Is.EqualTo("Attack"));
        }

        /// <summary>
        /// 当不存在可达目标的有效动作链时，A* 搜索返回空列表。
        /// </summary>
        [Test]
        public void GOAPPlanner_AStarSearch_NoPath_ReturnsNull()
        {
            GOAPWorldState startState = new GOAPWorldState(4);
            // 起始状态缺少必需的前置条件

            GOAPGoal goal = new GOAPGoal
            {
                Name = "KillEnemy",
                Priority = 10,
                GoalConditions = new Dictionary<string, bool>
                {
                    { "IsEnemyDead", true }
                }
            };

            GOAPAction attack = new GOAPAction
            {
                Name = "Attack",
                Cost = 2f,
                Preconditions = new Dictionary<string, bool>
                {
                    { "HasWeapon", true }
                },
                Effects = new Dictionary<string, bool>
                {
                    { "IsEnemyDead", true }
                }
            };

            List<GOAPAction> availableActions = new List<GOAPAction> { attack };

            List<GOAPAction> plan = GOAPPlanner.AStarSearch(startState, goal, availableActions);

            // 无路径时返回空列表（方法不返回 null）
            Assert.That(plan, Is.Not.Null);
            Assert.That(plan.Count, Is.EqualTo(0));
        }

        #endregion

        #region 策略级测试

        /// <summary>
        /// 完整的策略评估流程：设置目标、动作和世界状态前置条件后，
        /// Evaluate 应通过 A* 规划并返回第一个动作的指令。
        /// </summary>
        [Test]
        public void GOAPStrategy_Evaluate_ExecutesPlan()
        {
            GOAPGoal killEnemyGoal = new GOAPGoal
            {
                Name = "KillEnemy",
                Priority = 10,
                GoalConditions = new Dictionary<string, bool>
                {
                    { "IsEnemyDead", true }
                }
            };

            GOAPAction attackAction = new GOAPAction
            {
                Name = "Attack",
                Cost = 2f,
                Preconditions = new Dictionary<string, bool>
                {
                    { "HasWeapon", true }
                },
                Effects = new Dictionary<string, bool>
                {
                    { "IsEnemyDead", true }
                },
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    cmd.TypeName = "Wait";
                    return cmd;
                }
            };

            _strategy.AddGoal(killEnemyGoal);
            _strategy.AddAction(attackAction);

            // 设置世界状态使其满足攻击动作的前置条件
            _worldState.Set("HasWeapon", true);

            _strategy.Initialize();

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Wait"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 策略禁用后 Evaluate 返回空列表，即使目标与动作均已配置。
        /// </summary>
        [Test]
        public void GOAPStrategy_Evaluate_Disabled_ReturnsEmpty()
        {
            GOAPGoal goal = new GOAPGoal
            {
                Name = "Idle",
                Priority = 1,
                GoalConditions = new Dictionary<string, bool>
                {
                    { "IsIdle", true }
                }
            };

            GOAPAction action = new GOAPAction
            {
                Name = "Wait",
                Cost = 1f,
                Preconditions = new Dictionary<string, bool>(),
                Effects = new Dictionary<string, bool>
                {
                    { "IsIdle", true }
                },
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    return cmd;
                }
            };

            _strategy.AddGoal(goal);
            _strategy.AddAction(action);
            _strategy.Initialize();
            _strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 策略未注册任何目标时，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void GOAPStrategy_Evaluate_NoGoals_ReturnsEmpty()
        {
            GOAPAction action = new GOAPAction
            {
                Name = "Attack",
                Cost = 2f,
                Preconditions = new Dictionary<string, bool>(),
                Effects = new Dictionary<string, bool>(),
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    return cmd;
                }
            };

            _strategy.AddAction(action);
            _strategy.Initialize();

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
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
