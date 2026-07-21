using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    /// <summary>
    /// 效用响应曲线类型枚举。
    /// 定义如何将原始输入值映射到 0~1 的效用评分区间。
    /// </summary>
    public enum UtilityCurveType
    {
        /// <summary>
        /// 线性映射：y = x（输入直接作为输出，范围 [0, 1]）。
        /// </summary>
        Linear,

        /// <summary>
        /// S 形曲线：y = x² / (x² + (1 - x)²)，中间段变化平缓，两端变化剧烈。
        /// </summary>
        SShape,

        /// <summary>
        /// 指数增长曲线：y = x^power（power 可通过参数配置，默认 2）。
        /// </summary>
        Exponential,

        /// <summary>
        /// 反向映射：y = 1 - x（高输入值对应低效用）。
        /// </summary>
        Inverse,

        /// <summary>
        /// 阶梯函数：超过阈值返回 1，否则返回 0（threshold 可通过参数配置，默认 0.5）。
        /// </summary>
        Step
    }

    /// <summary>
    /// 效用响应曲线定义。
    /// 将原始评分值映射为 0~1 的标准化效用值，支持多种曲线类型和可配置参数。
    /// </summary>
    public class UtilityCurve
    {
        /// <summary>
        /// 曲线类型。
        /// </summary>
        public UtilityCurveType Type { get; set; }

        /// <summary>
        /// 曲线参数字典。不同曲线类型使用不同的参数键：
        /// <list type="bullet">
        /// <item><c>"power"</c> — <see cref="UtilityCurveType.Exponential"/> 的指数值，默认 2。</item>
        /// <item><c>"threshold"</c> — <see cref="UtilityCurveType.Step"/> 的判定阈值，默认 0.5。</item>
        /// </list>
        /// </summary>
        public Dictionary<string, float> Parameters { get; set; }

        /// <summary>
        /// 初始化一个默认线性曲线。
        /// </summary>
        public UtilityCurve()
        {
            Type = UtilityCurveType.Linear;
            Parameters = new Dictionary<string, float>();
        }

        /// <summary>
        /// 使用指定曲线类型初始化。
        /// </summary>
        /// <param name="type">曲线类型。</param>
        public UtilityCurve(UtilityCurveType type)
        {
            Type = type;
            Parameters = new Dictionary<string, float>();
        }

        /// <summary>
        /// 计算曲线映射值。将原始输入值根据曲线类型映射为 0~1 的效用评分。
        /// 输入值超出 [0, 1] 范围时自动钳制。
        /// </summary>
        /// <param name="rawValue">原始输入值，期望在 [0, 1] 范围内。</param>
        /// <returns>映射后的效用值，范围 [0, 1]。</returns>
        public float Evaluate(float rawValue)
        {
            // 钳制到 [0, 1]
            float x = Clamp01(rawValue);

            switch (Type)
            {
                case UtilityCurveType.Linear:
                    return x;

                case UtilityCurveType.SShape:
                    return EvaluateSShape(x);

                case UtilityCurveType.Exponential:
                    return EvaluateExponential(x);

                case UtilityCurveType.Inverse:
                    return 1f - x;

                case UtilityCurveType.Step:
                    return EvaluateStep(x);

                default:
                    return x;
            }
        }

        /// <summary>
        /// 获取曲线参数值，若不存在则返回默认值。
        /// </summary>
        /// <param name="key">参数键名。</param>
        /// <param name="defaultValue">默认值。</param>
        /// <returns>参数值或默认值。</returns>
        private float GetParam(string key, float defaultValue)
        {
            if (Parameters == null)
            {
                return defaultValue;
            }

            if (Parameters.TryGetValue(key, out float value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// S 形曲线计算：y = x² / (x² + (1 - x)²)。
        /// 当 x=0 时返回 0，x=1 时返回 1，x=0.5 时返回 0.5。
        /// 分母在 x 为任意值时均 ≥ 0.5，不会发生除零。
        /// </summary>
        private static float EvaluateSShape(float x)
        {
            float x2 = x * x;
            float oneMinusX2 = (1f - x) * (1f - x);
            float denominator = x2 + oneMinusX2;

            // denominator 最小值为 0.5（当 x=0.5 时），无需除零保护
            return x2 / denominator;
        }

        /// <summary>
        /// 指数增长曲线计算：y = x^power。
        /// 使用参数 "power"，默认值为 2。
        /// </summary>
        private float EvaluateExponential(float x)
        {
            float power = GetParam("power", 2f);

            // 边界情况：0^0 在数学上未定义，此处视为 0
            if (x <= 0f)
            {
                return 0f;
            }

            return (float)Math.Pow(x, power);
        }

        /// <summary>
        /// 阶梯函数计算：超过阈值返回 1，否则返回 0。
        /// 使用参数 "threshold"，默认值为 0.5。
        /// 输入值恰好等于阈值时视为超过。
        /// </summary>
        private float EvaluateStep(float x)
        {
            float threshold = GetParam("threshold", 0.5f);
            return x >= threshold ? 1f : 0f;
        }

        /// <summary>
        /// 将值钳制到 [0, 1] 范围。
        /// </summary>
        private static float Clamp01(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            if (value >= 1f)
            {
                return 1f;
            }

            return value;
        }
    }

    /// <summary>
    /// 效用因素 — 代表决策考量的一个维度。
    /// 通过评分函数从上下文中提取原始值，再通过响应曲线映射为标准化效用评分，
    /// 最终按权重参与总效用计算。
    /// </summary>
    public class UtilityFactor
    {
        /// <summary>
        /// 因素名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 因素在总效用中的权重，默认 1。
        /// 权重越高，该因素对最终决策的影响越大。
        /// </summary>
        public float Weight { get; set; } = 1f;

        /// <summary>
        /// 响应曲线，决定原始值到效用评分的映射方式。
        /// </summary>
        public UtilityCurve Curve { get; set; }

        /// <summary>
        /// 评分函数 — 从评估上下文中提取因素的原始值。
        /// 返回值应为 [0, 1] 范围内的标准化值，或由 <see cref="Curve"/> 负责映射。
        /// </summary>
        public Func<IEvaluationContext, float> ScoreFunction { get; set; }

        /// <summary>
        /// 初始化一个默认效用因素。
        /// </summary>
        public UtilityFactor()
        {
            Name = string.Empty;
            Curve = new UtilityCurve();
        }

        /// <summary>
        /// 评估此因素在当前上下文中的效用贡献。
        /// 计算过程：ScoreFunction 获取原始值 → Curve.Evaluate 映射 → 乘以权重。
        /// </summary>
        /// <param name="context">AI 评估上下文。</param>
        /// <returns>该因素的加权效用贡献值。</returns>
        public float Evaluate(IEvaluationContext context)
        {
            if (ScoreFunction == null)
            {
                return 0f;
            }

            float rawValue = ScoreFunction(context);

            if (Curve == null)
            {
                return rawValue * Weight;
            }

            float mappedValue = Curve.Evaluate(rawValue);
            return mappedValue * Weight;
        }
    }

    /// <summary>
    /// 效用行动 — 将一组效用因素与一个动作工厂打包。
    /// 每次评估时计算所有因素的加权平均效用，效用最高者被选中执行。
    /// </summary>
    public class UtilityAction
    {
        /// <summary>
        /// 行动名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 该行动关联的效用因素列表。
        /// </summary>
        public List<UtilityFactor> Factors { get; set; }

        /// <summary>
        /// 动作工厂 — 当此行动被选中时，创建对应的 <see cref="IActionCommand"/> 实例。
        /// </summary>
        public Func<IEvaluationContext, IActionCommand> ActionFactory { get; set; }

        /// <summary>
        /// 初始化一个默认效用行动。
        /// </summary>
        public UtilityAction()
        {
            Name = string.Empty;
            Factors = new List<UtilityFactor>();
        }

        /// <summary>
        /// 计算此行动在当前上下文中的总效用。
        /// 公式：总效用 = Σ(factor.Evaluate × Weight) / Σ(Weight)。
        /// 若未配置任何因素或所有因素权重之和为零，则返回 0。
        /// </summary>
        /// <param name="context">AI 评估上下文。</param>
        /// <returns>总效用值，范围 [0, 1]。</returns>
        public float CalculateUtility(IEvaluationContext context)
        {
            if (Factors == null || Factors.Count == 0)
            {
                return 0f;
            }

            float totalWeightedValue = 0f;
            float totalWeight = 0f;

            for (int i = 0; i < Factors.Count; i++)
            {
                UtilityFactor factor = Factors[i];

                if (factor == null)
                {
                    continue;
                }

                float weight = factor.Weight;

                // 跳过零权重因素，但需先检查以避免负权重干扰
                if (weight <= 0f)
                {
                    continue;
                }

                float factorValue = factor.Evaluate(context);
                totalWeightedValue += factorValue;
                totalWeight += weight;
            }

            if (totalWeight <= 0f)
            {
                return 0f;
            }

            return Clamp01(totalWeightedValue / totalWeight);
        }

        /// <summary>
        /// 将值钳制到 [0, 1] 范围。
        /// </summary>
        private static float Clamp01(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            if (value >= 1f)
            {
                return 1f;
            }

            return value;
        }
    }

    /// <summary>
    /// 效用系统决策策略 — 基于多因素加权评分的 AI 决策器。
    /// 每个 <see cref="UtilityAction"/> 包含多个 <see cref="UtilityFactor"/>，
    /// 每个因素由评分函数、响应曲线和权重组成。评估时对所有行动计算加权平均效用，
    /// 选择效用最高的行动，通过其 ActionFactory 生成 <see cref="IActionCommand"/>。
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// var strategy = new UtilityStrategy();
    /// var attackAction = new UtilityAction
    /// {
    ///     Name = "Attack",
    ///     ActionFactory = ctx => ReferencePool.Acquire&lt;YourAttackAction&gt;(),
    ///     Factors = new List&lt;UtilityFactor&gt;
    ///     {
    ///         new UtilityFactor
    ///         {
    ///             Name = "HealthRatio",
    ///             Weight = 1.5f,
    ///             Curve = new UtilityCurve(UtilityCurveType.Inverse),
    ///             ScoreFunction = ctx => ctx.Blackboard.Get&lt;float&gt;("health_ratio")
    ///         },
    ///         new UtilityFactor
    ///         {
    ///             Name = "DistanceToEnemy",
    ///             Weight = 1.0f,
    ///             Curve = new UtilityCurve(UtilityCurveType.Linear),
    ///             ScoreFunction = ctx => 1f - (ctx.Perception.NearestEnemyDistance / 100f)
    ///         }
    ///     }
    /// };
    /// strategy.AddAction(attackAction);
    /// strategy.Initialize();
    /// var commands = strategy.Evaluate(context);
    /// </code>
    /// </remarks>
    public class UtilityStrategy : IDecisionStrategy
    {
        private List<UtilityAction> _actions;
        private bool _initialized;

        /// <inheritdoc/>
        public string Name => "Utility";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 初始化一个空的效用策略。
        /// </summary>
        public UtilityStrategy()
        {
            _actions = new List<UtilityAction>();
        }

        /// <summary>
        /// 获取当前已注册的行动列表（只读）。
        /// </summary>
        public IReadOnlyList<UtilityAction> Actions => _actions;

        /// <summary>
        /// 添加一个效用行动到策略中。
        /// </summary>
        /// <param name="action">要添加的效用行动。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="action"/> 为 null 时抛出。</exception>
        public void AddAction(UtilityAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _actions.Add(action);
        }

        /// <summary>
        /// 移除一个效用行动。
        /// </summary>
        /// <param name="action">要移除的效用行动。</param>
        /// <returns>成功移除返回 true，若不存在返回 false。</returns>
        public bool RemoveAction(UtilityAction action)
        {
            return _actions.Remove(action);
        }

        /// <summary>
        /// 按名称移除效用行动。
        /// </summary>
        /// <param name="name">行动名称。</param>
        /// <returns>成功移除返回 true，若不存在返回 false。</returns>
        public bool RemoveActionByName(string name)
        {
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                if (_actions[i].Name == name)
                {
                    _actions.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清空所有已注册的效用行动。
        /// </summary>
        public void ClearActions()
        {
            _actions.Clear();
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            _initialized = true;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (!_initialized || !IsEnabled || context == null)
            {
                return Array.Empty<IActionCommand>();
            }

            if (_actions.Count == 0)
            {
                return Array.Empty<IActionCommand>();
            }

            UtilityAction bestAction = null;
            float bestUtility = float.MinValue;

            for (int i = 0; i < _actions.Count; i++)
            {
                UtilityAction action = _actions[i];

                if (action == null)
                {
                    continue;
                }

                float utility = action.CalculateUtility(context);

                // 严格大于才替换，保证效用相同时保留第一个
                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    bestAction = action;
                }
            }

            if (bestAction == null)
            {
                return Array.Empty<IActionCommand>();
            }

            if (bestAction.ActionFactory == null)
            {
                return Array.Empty<IActionCommand>();
            }

            IActionCommand command = bestAction.ActionFactory(context);

            if (command == null)
            {
                return Array.Empty<IActionCommand>();
            }

            return new IActionCommand[] { command };
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _actions.Clear();
            _initialized = false;
        }
    }
}
