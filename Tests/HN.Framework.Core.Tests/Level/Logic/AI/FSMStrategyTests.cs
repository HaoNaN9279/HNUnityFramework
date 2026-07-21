#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using System;
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
    using HN.Framework.Core.Level.Logic;
    using HN.Framework.Core.Level.Logic.AI;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using HN.Framework.Core.Level.Logic.AI.Strategies;
    using NUnit.Framework;

    /// <summary>
    /// FSMStrategy 单元测试 — 验证有限状态机策略的状态注册、转换评估、启停控制和资源释放。
    /// </summary>
    [TestFixture]
    public class FSMStrategyTests
    {
        private FSMStrategy _strategy = null!;
        private StrategyContext _context = null!;
        private Blackboard _blackboard = null!;
        private WorldStateCache _worldState = null!;
        private PerceptionState _perception = null!;

        /// <summary>
        /// 每个测试前初始化 FSM 策略和评估上下文。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _strategy = new FSMStrategy();
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
        /// 每个测试后重置策略和清理上下文资源。
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
        /// 添加状态并设置转换后，通过 Entry 状态的转换进入目标状态，Evaluate 返回对应动作。
        /// Entry 状态（isAllowedTrans=true）→ 用户状态转移 → 返回目标状态的动作指令。
        /// </summary>
        [Test]
        public void AddState_AndAddTransition_EvaluatesCorrectly()
        {
            HFSMState attackState = _strategy.AddState("Attack", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Attack";
                return cmd;
            });
            Assert.That(attackState, Is.Not.Null);
            Assert.That(attackState.Name, Is.EqualTo("Attack"));

            // 从 Entry 状态转移到 Attack 状态（Entry 允许转换）
            _strategy.AddTransition("Entry", "Attack", () => true);
            _strategy.SetStartState("Entry");

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Attack"));
            Assert.That(_strategy.CurrentState, Is.Not.Null);
            Assert.That(_strategy.CurrentState!.Name, Is.EqualTo("Attack"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 当转换条件为 true 时，通过 Entry 状态自动转移到目标状态。
        /// </summary>
        [Test]
        public void Evaluate_WithConditionMet_TransitionsToNextState()
        {
            _strategy.AddState("Combat", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Attack";
                return cmd;
            });
            _strategy.AddTransition("Entry", "Combat", () => true);
            _strategy.SetStartState("Entry");

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            // 转换已发生，当前状态应为 Combat
            Assert.That(_strategy.CurrentState, Is.Not.Null);
            Assert.That(_strategy.CurrentState!.Name, Is.EqualTo("Combat"));
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Attack"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 当转换条件为 false 时，FSM 保持在 Entry 状态不转移，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void Evaluate_WithConditionNotMet_StaysInCurrentState()
        {
            _strategy.AddState("Combat", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Attack";
                return cmd;
            });
            _strategy.AddTransition("Entry", "Combat", () => false);
            _strategy.SetStartState("Entry");

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            // Entry 状态未绑定动作工厂，转换也未触发
            Assert.That(_strategy.CurrentState, Is.Not.Null);
            Assert.That(_strategy.CurrentState!.Name, Is.EqualTo("Entry"));
            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 策略启用且 FSM 运行中，Evaluate 返回当前状态的动作指令。
        /// </summary>
        [Test]
        public void Evaluate_EnabledAndRunning_ReturnsCurrentStateAction()
        {
            _strategy.AddState("Idle", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Wait";
                return cmd;
            });
            _strategy.SetStartState("Idle");

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(_strategy.IsRunning, Is.True);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Wait"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 策略禁用后，Evaluate 返回空列表且不驱动 FSM。
        /// </summary>
        [Test]
        public void Evaluate_Disabled_ReturnsEmpty()
        {
            _strategy.AddState("Idle", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            });
            _strategy.SetStartState("Idle");
            _strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Reset 后 FSM 释放到引用池，状态映射清空。
        /// </summary>
        [Test]
        public void Reset_ReleasesFSM()
        {
            _strategy.AddState("Idle", ctx =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                return cmd;
            });
            _strategy.SetStartState("Idle");

            Assert.That(_strategy.IsRunning, Is.True);

            _strategy.Reset();

            Assert.That(_strategy.IsRunning, Is.False);
            Assert.That(_strategy.CurrentState, Is.Null);

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);
            Assert.That(result.Count, Is.EqualTo(0));
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
