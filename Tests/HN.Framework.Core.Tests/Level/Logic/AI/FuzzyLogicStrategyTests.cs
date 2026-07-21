#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using System;
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
    using HN.Framework.Core.Level.Logic.AI;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using HN.Framework.Core.Level.Logic.AI.Strategies;
    using NUnit.Framework;

    /// <summary>
    /// FuzzyLogicStrategy 单元测试 — 验证模糊数学工具函数、模糊集合/变量和 Mamdani 推理策略。
    /// </summary>
    [TestFixture]
    public class FuzzyLogicStrategyTests
    {
        private FuzzyLogicStrategy _strategy = null!;
        private StrategyContext _context = null!;
        private Blackboard _blackboard = null!;
        private WorldStateCache _worldState = null!;
        private PerceptionState _perception = null!;

        /// <summary>
        /// 每个测试前初始化模糊逻辑策略和评估上下文。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _strategy = new FuzzyLogicStrategy();

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

        #region FuzzyMath 测试

        /// <summary>
        /// 三角隶属度函数在峰值中心 x0 处返回 1。
        /// </summary>
        [Test]
        public void FuzzyMath_Triangle_PeakAtCenter_Returns1()
        {
            float result = FuzzyMath.Triangle(50f, 50f, 30f);

            Assert.That(result, Is.EqualTo(1f));
        }

        /// <summary>
        /// 三角隶属度函数在基底边缘 x0±d 处返回 0。
        /// </summary>
        [Test]
        public void FuzzyMath_Triangle_AtEdges_Returns0()
        {
            float leftEdge = FuzzyMath.Triangle(20f, 50f, 30f);
            float rightEdge = FuzzyMath.Triangle(80f, 50f, 30f);
            float outside = FuzzyMath.Triangle(10f, 50f, 30f);

            Assert.That(leftEdge, Is.EqualTo(0f));
            Assert.That(rightEdge, Is.EqualTo(0f));
            Assert.That(outside, Is.EqualTo(0f));
        }

        /// <summary>
        /// 梯形隶属度函数在平台段 [x1, x2] 返回 1。
        /// </summary>
        [Test]
        public void FuzzyMath_Trapezoid_FlatRegion_Returns1()
        {
            float result = FuzzyMath.Trapezoid(50f, 20f, 40f, 60f, 80f);

            Assert.That(result, Is.EqualTo(1f));
        }

        #endregion

        #region FuzzySet 测试

        /// <summary>
        /// FuzzySet 使用三角隶属度函数计算正确的隶属度。
        /// </summary>
        [Test]
        public void FuzzySet_ComputeMembership_Triangle_Correct()
        {
            FuzzySet set = new FuzzySet("Medium", MembershipFunctionType.Triangle, 50f, 30f);

            float peak = set.ComputeMembership(50f);
            float edge = set.ComputeMembership(80f);
            float outside = set.ComputeMembership(10f);

            Assert.That(peak, Is.EqualTo(1f));
            Assert.That(edge, Is.EqualTo(0f));
            Assert.That(outside, Is.EqualTo(0f));
        }

        #endregion

        #region FuzzyVariable 测试

        /// <summary>
        /// FuzzyVariable.Fuzzify 返回与已注册集合对应的正确隶属度向量。
        /// </summary>
        [Test]
        public void FuzzyVariable_Fuzzify_ReturnsCorrectMembership()
        {
            FuzzyVariable variable = new FuzzyVariable("distance", 0f, 100f);
            variable.AddSet(new FuzzySet("Near", MembershipFunctionType.Trapezoid, 0f, 0f, 20f, 40f));
            variable.AddSet(new FuzzySet("Mid", MembershipFunctionType.Triangle, 50f, 30f));
            variable.AddSet(new FuzzySet("Far", MembershipFunctionType.Trapezoid, 60f, 80f, 100f, 100f));

            List<float> memberships = variable.Fuzzify(50f);

            Assert.That(memberships.Count, Is.EqualTo(3));
            Assert.That(memberships[0], Is.EqualTo(0f));  // Near: 50f outside [0,40]
            Assert.That(memberships[1], Is.EqualTo(1f));  // Mid: 50f is peak
            Assert.That(memberships[2], Is.EqualTo(0f));  // Far: 50f outside [60,100]
        }

        #endregion

        #region FuzzyLogicStrategy 测试

        /// <summary>
        /// 配置完整的模糊推理系统后，Evaluate 返回 OutputMapper 产生的动作指令。
        /// </summary>
        [Test]
        public void FuzzyLogicStrategy_Evaluate_WithRules_ReturnsCommand()
        {
            // 输入变量 "distance"
            FuzzyVariable distanceVar = new FuzzyVariable("distance", 0f, 100f);
            distanceVar.AddSet(new FuzzySet("Near", MembershipFunctionType.Trapezoid, 0f, 0f, 20f, 40f));
            distanceVar.AddSet(new FuzzySet("Far", MembershipFunctionType.Trapezoid, 60f, 80f, 100f, 100f));
            _strategy.AddInputVariable(distanceVar);
            _strategy.SetInputProvider("distance", ctx => 10f); // distance=10 → Near membership high

            // 输出变量 "aggressiveness"
            FuzzyVariable outputVar = new FuzzyVariable("aggressiveness", 0f, 1f);
            outputVar.AddSet(new FuzzySet("Low", MembershipFunctionType.Trapezoid, 0f, 0f, 0.2f, 0.4f));
            outputVar.AddSet(new FuzzySet("High", MembershipFunctionType.Trapezoid, 0.6f, 0.8f, 1f, 1f));
            _strategy.SetOutputVariable(outputVar);

            // 规则：IF distance is Near THEN aggressiveness is High
            _strategy.AddRule(new FuzzyRule(
                "R1",
                new List<(string, int)> { ("distance", 0) }, // Near = index 0
                ("aggressiveness", 1)));                      // High = index 1

            // 输出映射
            _strategy.SetOutputMapper(v =>
            {
                var cmd = ReferencePool.Acquire<TestActionCommand>();
                cmd.Initialize();
                cmd.TypeName = "Attack";
                return cmd;
            });

            _strategy.Initialize();

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Attack"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 策略禁用后，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void FuzzyLogicStrategy_Evaluate_Disabled_ReturnsEmpty()
        {
            _strategy.Initialize();
            _strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 未添加任何规则时 Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void FuzzyLogicStrategy_Evaluate_NoRules_ReturnsEmpty()
        {
            FuzzyVariable outputVar = new FuzzyVariable("output", 0f, 1f);
            outputVar.AddSet(new FuzzySet("Any", MembershipFunctionType.Triangle, 0.5f, 0.5f));
            _strategy.SetOutputVariable(outputVar);
            _strategy.SetOutputMapper(v => null);
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
