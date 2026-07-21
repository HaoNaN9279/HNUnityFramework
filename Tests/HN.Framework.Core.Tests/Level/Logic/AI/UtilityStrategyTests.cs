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
    /// UtilityStrategy 单元测试 — 验证效用曲线数学、效用因素/行动计算和效用系统决策策略。
    /// </summary>
    [TestFixture]
    public class UtilityStrategyTests
    {
        private UtilityStrategy _strategy = null!;
        private StrategyContext _context = null!;
        private Blackboard _blackboard = null!;
        private WorldStateCache _worldState = null!;
        private PerceptionState _perception = null!;

        /// <summary>
        /// 每个测试前初始化效用策略和评估上下文。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _strategy = new UtilityStrategy();

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

        #region UtilityCurve 测试

        /// <summary>
        /// 线性曲线返回钳制到 [0, 1] 的原始值。
        /// </summary>
        [Test]
        public void UtilityCurve_Linear_ReturnsClampedValue()
        {
            UtilityCurve curve = new UtilityCurve(UtilityCurveType.Linear);

            Assert.That(curve.Evaluate(0.5f), Is.EqualTo(0.5f));
            Assert.That(curve.Evaluate(2.0f), Is.EqualTo(1f));
            Assert.That(curve.Evaluate(-0.5f), Is.EqualTo(0f));
        }

        /// <summary>
        /// S 形曲线在中间值附近平滑过渡，端点附近变化剧烈。
        /// </summary>
        [Test]
        public void UtilityCurve_SShape_ReturnsSigmoidValue()
        {
            UtilityCurve curve = new UtilityCurve(UtilityCurveType.SShape);

            Assert.That(curve.Evaluate(0f), Is.EqualTo(0f));
            Assert.That(curve.Evaluate(1f), Is.EqualTo(1f));

            float mid = curve.Evaluate(0.5f);
            Assert.That(mid, Is.EqualTo(0.5f).Within(0.001f));

            Assert.That(curve.Evaluate(0.2f), Is.LessThan(curve.Evaluate(0.8f)));
        }

        /// <summary>
        /// 阶梯函数在输入超过阈值时返回 1，低于阈值时返回 0。
        /// </summary>
        [Test]
        public void UtilityCurve_Step_AboveThreshold_Returns1()
        {
            UtilityCurve curve = new UtilityCurve(UtilityCurveType.Step);
            curve.Parameters["threshold"] = 0.5f;

            Assert.That(curve.Evaluate(0.5f), Is.EqualTo(1f));  // >= threshold
            Assert.That(curve.Evaluate(0.8f), Is.EqualTo(1f));
            Assert.That(curve.Evaluate(0.49f), Is.EqualTo(0f));
        }

        #endregion

        #region UtilityFactor 测试

        /// <summary>
        /// UtilityFactor 通过 ScoreFunction 获取原始值，经曲线映射后乘以权重。
        /// </summary>
        [Test]
        public void UtilityFactor_Evaluate_ReturnsWeightedValue()
        {
            UtilityFactor factor = new UtilityFactor
            {
                Name = "Distance",
                Weight = 2.0f,
                Curve = new UtilityCurve(UtilityCurveType.Linear),
                ScoreFunction = ctx => 0.75f
            };

            float result = factor.Evaluate(_context);

            // 0.75 * 1.0 (linear) * 2.0 = 1.5
            Assert.That(result, Is.EqualTo(1.5f));
        }

        #endregion

        #region UtilityAction 测试

        /// <summary>
        /// UtilityAction.CalculateUtility 计算所有因素加权平均效用值。
        /// </summary>
        [Test]
        public void UtilityAction_CalculateUtility_ReturnsWeightedAverage()
        {
            UtilityAction action = new UtilityAction
            {
                Name = "Flee",
                Factors = new List<UtilityFactor>
                {
                    new UtilityFactor
                    {
                        Name = "Health",
                        Weight = 2.0f,
                        Curve = new UtilityCurve(UtilityCurveType.Linear),
                        ScoreFunction = ctx => 0.3f
                    },
                    new UtilityFactor
                    {
                        Name = "Threat",
                        Weight = 1.0f,
                        Curve = new UtilityCurve(UtilityCurveType.Linear),
                        ScoreFunction = ctx => 0.9f
                    }
                }
            };

            float utility = action.CalculateUtility(_context);

            // totalWeighted = (0.3 * 2.0) + (0.9 * 1.0) = 0.6 + 0.9 = 1.5
            // totalWeight = 2.0 + 1.0 = 3.0
            // utility = 1.5 / 3.0 = 0.5
            Assert.That(utility, Is.EqualTo(0.5f));
        }

        #endregion

        #region UtilityStrategy 测试

        /// <summary>
        /// 多个行动中，Evaluate 选择效用最高的并返回其动作指令。
        /// </summary>
        [Test]
        public void UtilityStrategy_Evaluate_SelectsHighestUtility()
        {
            // 低效用行动
            UtilityAction lowAction = new UtilityAction
            {
                Name = "Idle",
                Factors = new List<UtilityFactor>
                {
                    new UtilityFactor
                    {
                        Name = "Boredom",
                        Weight = 1.0f,
                        Curve = new UtilityCurve(UtilityCurveType.Linear),
                        ScoreFunction = ctx => 0.2f
                    }
                },
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    cmd.TypeName = "Wait";
                    return cmd;
                }
            };

            // 高效用行动
            UtilityAction highAction = new UtilityAction
            {
                Name = "Attack",
                Factors = new List<UtilityFactor>
                {
                    new UtilityFactor
                    {
                        Name = "Aggro",
                        Weight = 1.0f,
                        Curve = new UtilityCurve(UtilityCurveType.Linear),
                        ScoreFunction = ctx => 0.9f
                    }
                },
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    cmd.TypeName = "Attack";
                    return cmd;
                }
            };

            _strategy.AddAction(lowAction);
            _strategy.AddAction(highAction);
            _strategy.Initialize();

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Attack"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 效用相同时，选择第一个注册的行动（严格大于判断确保稳定性）。
        /// </summary>
        [Test]
        public void UtilityStrategy_Evaluate_TiedUtility_SelectsFirst()
        {
            UtilityAction firstAction = new UtilityAction
            {
                Name = "Patrol",
                Factors = new List<UtilityFactor>
                {
                    new UtilityFactor
                    {
                        Name = "Need",
                        Weight = 1.0f,
                        Curve = new UtilityCurve(UtilityCurveType.Linear),
                        ScoreFunction = ctx => 0.5f
                    }
                },
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    cmd.TypeName = "Wait";
                    return cmd;
                }
            };

            UtilityAction secondAction = new UtilityAction
            {
                Name = "Guard",
                Factors = new List<UtilityFactor>
                {
                    new UtilityFactor
                    {
                        Name = "Need",
                        Weight = 1.0f,
                        Curve = new UtilityCurve(UtilityCurveType.Linear),
                        ScoreFunction = ctx => 0.5f
                    }
                },
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    cmd.TypeName = "Attack";
                    return cmd;
                }
            };

            _strategy.AddAction(firstAction);
            _strategy.AddAction(secondAction);
            _strategy.Initialize();

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            // 效用相同，应选第一个
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TypeName, Is.EqualTo("Wait"));

            ReferencePool.Release(result[0]);
        }

        /// <summary>
        /// 策略禁用后，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void UtilityStrategy_Evaluate_Disabled_ReturnsEmpty()
        {
            _strategy.AddAction(new UtilityAction
            {
                Name = "Test",
                Factors = new List<UtilityFactor>
                {
                    new UtilityFactor
                    {
                        Weight = 1.0f,
                        Curve = new UtilityCurve(UtilityCurveType.Linear),
                        ScoreFunction = ctx => 1.0f
                    }
                },
                ActionFactory = ctx =>
                {
                    var cmd = ReferencePool.Acquire<TestActionCommand>();
                    cmd.Initialize();
                    return cmd;
                }
            });
            _strategy.Initialize();
            _strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = _strategy.Evaluate(_context);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 未注册任何行动时，Evaluate 返回空列表。
        /// </summary>
        [Test]
        public void UtilityStrategy_Evaluate_NoActions_ReturnsEmpty()
        {
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
