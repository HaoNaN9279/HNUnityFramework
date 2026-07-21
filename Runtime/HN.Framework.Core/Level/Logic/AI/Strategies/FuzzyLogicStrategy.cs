namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    using System;
    using System.Collections.Generic;
    using HN.Framework.Core.Level.Logic.AI;

    /// <summary>
    /// 隶属度函数类型枚举。
    /// 定义模糊变量中模糊集合的隶属度函数形状。
    /// </summary>
    public enum MembershipFunctionType
    {
        /// <summary>
        /// 三角隶属度函数。峰值为 1，向两侧线性递减至 0。
        /// 参数：x0（峰值位置），d（半底宽）。
        /// </summary>
        Triangle,

        /// <summary>
        /// 梯形隶属度函数。平台段为 1，两侧线性过渡至 0。
        /// 参数：x0（上升起点），x1（上升终点/平台起点），x2（平台终点/下降起点），x3（下降终点）。
        /// </summary>
        Trapezoid
    }

    /// <summary>
    /// 模糊数学工具类 — 提供标准隶属度函数的纯数学实现。
    /// 所有方法均为纯函数，无副作用，可用于高频调用的 Evaluate 流程。
    /// </summary>
    public static class FuzzyMath
    {
        /// <summary>
        /// 三角隶属度函数。峰值为 x0 处隶属度 = 1，基底从 x0-d 到 x0+d。
        /// 函数形状：从 (x0-d, 0) 线性上升至 (x0, 1)，再线性下降至 (x0+d, 0)。
        /// </summary>
        /// <param name="x">输入值。</param>
        /// <param name="x0">峰值位置。</param>
        /// <param name="d">半底宽（基底宽度的一半）。必须为正值。</param>
        /// <returns>隶属度，范围 [0, 1]。</returns>
        public static float Triangle(float x, float x0, float d)
        {
            // 当 d 为零或负时，仅在 x == x0 处隶属度为 1
            if (d <= 0f)
            {
                return x == x0 ? 1f : 0f;
            }

            float left = x0 - d;
            float right = x0 + d;

            // 在基底之外，隶属度为 0
            if (x <= left || x >= right)
            {
                return 0f;
            }

            // 左半段：从 (left, 0) 线性上升至 (x0, 1)
            if (x < x0)
            {
                return (x - left) / (x0 - left);
            }

            // 右半段：从 (x0, 1) 线性下降至 (right, 0)
            return (right - x) / (right - x0);
        }

        /// <summary>
        /// 梯形隶属度函数。平台段 [x1, x2] 隶属度恒为 1，
        /// 两侧从 0 线性过渡至 1（上升沿）和从 1 线性过渡至 0（下降沿）。
        /// </summary>
        /// <param name="x">输入值。</param>
        /// <param name="x0">上升沿起点（隶属度 0）。</param>
        /// <param name="x1">上升沿终点（隶属度 1）/平台起点。</param>
        /// <param name="x2">平台终点/下降沿起点（隶属度 1）。</param>
        /// <param name="x3">下降沿终点（隶属度 0）。</param>
        /// <returns>隶属度，范围 [0, 1]。</returns>
        /// <remarks>
        /// 参数必须满足 x0 ≤ x1 ≤ x2 ≤ x3。若不满足，结果未定义。
        /// 当 x0 == x1 时上升沿消失（直接从 0 跳到 1）；当 x2 == x3 时下降沿消失（直接从 1 跳到 0）。
        /// </remarks>
        public static float Trapezoid(float x, float x0, float x1, float x2, float x3)
        {
            // 在函数范围之外，隶属度为 0
            if (x <= x0 || x >= x3)
            {
                return 0f;
            }

            // 平台段：隶属度恒为 1
            if (x >= x1 && x <= x2)
            {
                return 1f;
            }

            // 上升沿：[x0, x1]，从 0 线性上升至 1
            if (x < x1)
            {
                float range = x1 - x0;
                if (range <= 0f)
                {
                    return 1f;
                }

                return (x - x0) / range;
            }

            // 下降沿：[x2, x3]，从 1 线性下降至 0
            float fallRange = x3 - x2;
            if (fallRange <= 0f)
            {
                return 1f;
            }

            return (x3 - x) / fallRange;
        }
    }

    /// <summary>
    /// 模糊集合 — 定义一个模糊语言值（如"近"、"远"）。
    /// 通过 <see cref="Type"/> 和 <see cref="Params"/> 描述隶属度函数形状，
    /// 调用 <see cref="ComputeMembership"/> 计算给定输入值在此集合中的隶属度。
    /// </summary>
    public class FuzzySet
    {
        /// <summary>
        /// 模糊集合名称（如 "Near"、"Medium"、"Far"）。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 隶属度函数类型。
        /// </summary>
        public MembershipFunctionType Type { get; set; }

        /// <summary>
        /// 隶属度函数参数数组。
        /// <list type="bullet">
        /// <item><see cref="MembershipFunctionType.Triangle"/>：<c>Params[0] = x0（峰值），Params[1] = d（半底宽）</c></item>
        /// <item><see cref="MembershipFunctionType.Trapezoid"/>：<c>Params[0]=x0, Params[1]=x1, Params[2]=x2, Params[3]=x3</c></item>
        /// </list>
        /// </summary>
        public float[] Params { get; set; }

        /// <summary>
        /// 初始化一个默认的三角模糊集合。
        /// </summary>
        public FuzzySet()
        {
            Name = string.Empty;
            Type = MembershipFunctionType.Triangle;
            Params = Array.Empty<float>();
        }

        /// <summary>
        /// 使用指定名称、类型和参数初始化模糊集合。
        /// </summary>
        /// <param name="name">集合名称。</param>
        /// <param name="type">隶属度函数类型。</param>
        /// <param name="parameters">隶属度函数参数数组。</param>
        public FuzzySet(string name, MembershipFunctionType type, params float[] parameters)
        {
            Name = name ?? string.Empty;
            Type = type;
            Params = parameters ?? Array.Empty<float>();
        }

        /// <summary>
        /// 计算输入值 x 在当前模糊集合中的隶属度。
        /// 根据 <see cref="Type"/> 选择对应的隶属度函数并计算结果。
        /// </summary>
        /// <param name="x">输入值。</param>
        /// <returns>隶属度，范围 [0, 1]。</returns>
        public float ComputeMembership(float x)
        {
            switch (Type)
            {
                case MembershipFunctionType.Triangle:
                {
                    if (Params == null || Params.Length < 2)
                    {
                        return 0f;
                    }

                    return FuzzyMath.Triangle(x, Params[0], Params[1]);
                }

                case MembershipFunctionType.Trapezoid:
                {
                    if (Params == null || Params.Length < 4)
                    {
                        return 0f;
                    }

                    return FuzzyMath.Trapezoid(x, Params[0], Params[1], Params[2], Params[3]);
                }

                default:
                    return 0f;
            }
        }
    }

    /// <summary>
    /// 模糊变量 — 定义一个输入或输出维度的模糊划分。
    /// 一个模糊变量包含若干 <see cref="FuzzySet"/>，覆盖其值域 [<see cref="MinValue"/>, <see cref="MaxValue"/>]。
    /// 通过 <see cref="Fuzzify"/> 将精确值转换为各集合的隶属度向量。
    /// </summary>
    public class FuzzyVariable
    {
        /// <summary>
        /// 变量名称（如 "distance"、"health"）。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 变量值域下界。
        /// </summary>
        public float MinValue { get; set; }

        /// <summary>
        /// 变量值域上界。
        /// </summary>
        public float MaxValue { get; set; }

        /// <summary>
        /// 该变量包含的模糊集合列表。顺序与 <see cref="Fuzzify"/> 返回的隶属度顺序一致。
        /// </summary>
        public List<FuzzySet> Sets { get; set; }

        /// <summary>
        /// 初始化一个空的模糊变量。
        /// </summary>
        public FuzzyVariable()
        {
            Name = string.Empty;
            Sets = new List<FuzzySet>();
        }

        /// <summary>
        /// 使用指定名称和值域初始化模糊变量。
        /// </summary>
        /// <param name="name">变量名称。</param>
        /// <param name="minValue">值域下界。</param>
        /// <param name="maxValue">值域上界。</param>
        public FuzzyVariable(string name, float minValue, float maxValue)
        {
            Name = name ?? string.Empty;
            MinValue = minValue;
            MaxValue = maxValue;
            Sets = new List<FuzzySet>();
        }

        /// <summary>
        /// 添加一个模糊集合到此变量中。
        /// 添加顺序决定 <see cref="Fuzzify"/> 返回的隶属度向量中各分量的索引。
        /// </summary>
        /// <param name="set">要添加的模糊集合。不可为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="set"/> 为 null 时抛出。</exception>
        public void AddSet(FuzzySet set)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            Sets.Add(set);
        }

        /// <summary>
        /// 模糊化 — 将精确值 value 转换为各模糊集合的隶属度向量。
        /// 返回的 <see cref="List{T}"/> 中第 i 个元素对应 <see cref="Sets"/> 中第 i 个集合的隶属度。
        /// </summary>
        /// <param name="value">精确输入值。</param>
        /// <returns>
        /// 隶属度向量，每个元素 ∈ [0, 1]，顺序与 <see cref="Sets"/> 一致。
        /// 若 <see cref="Sets"/> 为空则返回空列表。
        /// </returns>
        public List<float> Fuzzify(float value)
        {
            if (Sets == null || Sets.Count == 0)
            {
                return new List<float>();
            }

            var memberships = new List<float>(Sets.Count);

            for (int i = 0; i < Sets.Count; i++)
            {
                FuzzySet set = Sets[i];

                if (set == null)
                {
                    memberships.Add(0f);
                    continue;
                }

                memberships.Add(set.ComputeMembership(value));
            }

            return memberships;
        }
    }

    /// <summary>
    /// 模糊规则 — If-Then 形式的产生式规则。
    /// 前件（<see cref="Antecedents"/>）由多个 (变量名, 集合索引) 对组成，取 min 作为激活度；
    /// 后件（<see cref="Consequent"/>）指定激活的输出变量及集合索引。
    /// </summary>
    public class FuzzyRule
    {
        /// <summary>
        /// 规则名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 前件列表。每个元素为 (变量名, 集合索引) 对，
        /// 表示"变量 varName 的 SetIndex 号集合"为前件之一。
        /// 多个前件之间为 AND 关系（取 min 激活度）。
        /// </summary>
        public List<(string varName, int setIndex)> Antecedents { get; set; }

        /// <summary>
        /// 后件。指定规则的输出为 (变量名, 集合索引) 对。
        /// 规则激活时，以激活度裁剪该输出集合的隶属度函数。
        /// </summary>
        public (string varName, int setIndex) Consequent { get; set; }

        /// <summary>
        /// 初始化一条空的模糊规则。
        /// </summary>
        public FuzzyRule()
        {
            Name = string.Empty;
            Antecedents = new List<(string varName, int setIndex)>();
        }

        /// <summary>
        /// 使用指定名称、前件和后件初始化模糊规则。
        /// </summary>
        /// <param name="name">规则名称。</param>
        /// <param name="antecedents">前件列表。</param>
        /// <param name="consequent">后件。</param>
        public FuzzyRule(
            string name,
            List<(string varName, int setIndex)> antecedents,
            (string varName, int setIndex) consequent)
        {
            Name = name ?? string.Empty;
            Antecedents = antecedents ?? new List<(string varName, int setIndex)>();
            Consequent = consequent;
        }
    }

    /// <summary>
    /// 模糊逻辑决策策略 — 基于 Mamdani 模糊推理系统的 AI 决策器。
    /// 将输入变量模糊化，通过模糊规则库进行推理，经 MAX 聚合和重心法解模糊化后，
    /// 由 <see cref="OutputMapper"/> 转换为 <see cref="IActionCommand"/> 输出。
    /// </summary>
    /// <remarks>
    /// 推理流程（Mamdani）：
    /// <list type="number">
    ///   <item>输入获取：从 <see cref="InputProvider"/> 委托获取各输入变量的精确值</item>
    ///   <item>模糊化：各输入值通过对应 <see cref="FuzzyVariable"/> 的 <see cref="FuzzyVariable.Fuzzify"/> 转换为隶属度向量</item>
    ///   <item>规则激活：每条规则的前件隶属度取 min 作为激活度</item>
    ///   <item>后件裁剪：以激活度裁剪后件集合的隶属度函数（取 min）</item>
    ///   <item>MAX 聚合：对所有激活规则的后件隶属度取逐点最大值</item>
    ///   <item>解模糊化：重心法（COG）计算精确输出值</item>
    ///   <item>输出映射：通过 <see cref="OutputMapper"/> 将精确值转换为 <see cref="IActionCommand"/></item>
    /// </list>
    ///
    /// 使用示例：
    /// <code>
    /// var strategy = new FuzzyLogicStrategy();
    ///
    /// // 定义输入变量 "distance"
    /// var distanceVar = new FuzzyVariable("distance", 0f, 100f);
    /// distanceVar.AddSet(new FuzzySet("Near", MembershipFunctionType.Trapezoid, 0f, 0f, 20f, 40f));
    /// distanceVar.AddSet(new FuzzySet("Medium", MembershipFunctionType.Triangle, 50f, 30f));
    /// distanceVar.AddSet(new FuzzySet("Far", MembershipFunctionType.Trapezoid, 60f, 80f, 100f, 100f));
    /// strategy.AddInputVariable(distanceVar);
    ///
    /// // 定义输出变量 "aggressiveness"
    /// var outputVar = new FuzzyVariable("aggressiveness", 0f, 1f);
    /// outputVar.AddSet(new FuzzySet("Low", MembershipFunctionType.Trapezoid, 0f, 0f, 0.2f, 0.4f));
    /// outputVar.AddSet(new FuzzySet("Medium", MembershipFunctionType.Triangle, 0.5f, 0.3f));
    /// outputVar.AddSet(new FuzzySet("High", MembershipFunctionType.Trapezoid, 0.6f, 0.8f, 1f, 1f));
    /// strategy.SetOutputVariable(outputVar);
    ///
    /// // 添加规则
    /// strategy.AddRule(new FuzzyRule("R1",
    ///     new List&lt;(string, int)&gt; { ("distance", 0) },
    ///     ("aggressiveness", 2))); // IF distance is Near THEN aggressiveness is High
    /// strategy.AddRule(new FuzzyRule("R2",
    ///     new List&lt;(string, int)&gt; { ("distance", 2) },
    ///     ("aggressiveness", 0))); // IF distance is Far THEN aggressiveness is Low
    ///
    /// // 设置输入提供者和输出映射
    /// strategy.SetInputProvider("distance", ctx => ctx.Perception.NearestEnemyDistance);
    /// strategy.SetOutputMapper(v => v > 0.5f ? ReferencePool.Acquire&lt;YourAttackAction&gt;() :
    ///     ReferencePool.Acquire&lt;YourIdleAction&gt;());
    ///
    /// strategy.Initialize();
    /// var commands = strategy.Evaluate(context);
    /// </code>
    /// </remarks>
    public class FuzzyLogicStrategy : IDecisionStrategy
    {
        /// <summary>
        /// 解模糊化时对输出变量值域的采样点数。
        /// 数值越大精度越高，但计算开销也越大。
        /// </summary>
        private const int DefuzzificationSamples = 200;

        /// <inheritdoc/>
        public string Name => "FuzzyLogic";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 输入变量字典。键为变量名，值为对应的 <see cref="FuzzyVariable"/> 实例。
        /// </summary>
        public Dictionary<string, FuzzyVariable> InputVariables { get; private set; }

        /// <summary>
        /// 输出变量。Mamdani 推理中所有规则的后件均作用于该变量。
        /// </summary>
        public FuzzyVariable OutputVariable { get; private set; }

        /// <summary>
        /// 模糊规则库。
        /// </summary>
        public List<FuzzyRule> Rules { get; private set; }

        /// <summary>
        /// 输入提供者字典。键为变量名，值为从 <see cref="IEvaluationContext"/> 提取精确值的委托。
        /// </summary>
        public Dictionary<string, Func<IEvaluationContext, float>> InputProvider { get; private set; }

        /// <summary>
        /// 输出映射器。将解模糊化后的精确值转换为 <see cref="IActionCommand"/>。
        /// </summary>
        public Func<float, IActionCommand> OutputMapper { get; private set; }

        private bool _initialized;

        /// <summary>
        /// 初始化一个空的模糊逻辑策略。
        /// </summary>
        public FuzzyLogicStrategy()
        {
            InputVariables = new Dictionary<string, FuzzyVariable>();
            Rules = new List<FuzzyRule>();
            InputProvider = new Dictionary<string, Func<IEvaluationContext, float>>();
        }

        #region 配置方法

        /// <summary>
        /// 添加一个输入变量到策略中。
        /// 变量名必须与 <see cref="SetInputProvider"/> 中的键以及 <see cref="FuzzyRule"/> 前件中的变量名匹配。
        /// </summary>
        /// <param name="variable">要添加的输入变量。不可为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="variable"/> 为 null 时抛出。</exception>
        public void AddInputVariable(FuzzyVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            InputVariables[variable.Name] = variable;
        }

        /// <summary>
        /// 设置输出变量。策略仅支持单一输出变量。
        /// 重复调用将覆盖之前设置的输出变量。
        /// </summary>
        /// <param name="variable">输出变量。不可为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="variable"/> 为 null 时抛出。</exception>
        public void SetOutputVariable(FuzzyVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            OutputVariable = variable;
        }

        /// <summary>
        /// 添加一条模糊规则到规则库中。
        /// 规则的前件变量名必须与已注册的输入变量名匹配，后件变量名必须与输出变量名匹配。
        /// </summary>
        /// <param name="rule">要添加的模糊规则。不可为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="rule"/> 为 null 时抛出。</exception>
        public void AddRule(FuzzyRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            Rules.Add(rule);
        }

        /// <summary>
        /// 设置指定输入变量的值提供者。
        /// 每次 <see cref="Evaluate"/> 调用时，通过此委托从上下文中获取该变量的精确值。
        /// </summary>
        /// <param name="variableName">变量名称，必须与已注册的 <see cref="FuzzyVariable.Name"/> 一致。</param>
        /// <param name="provider">
        /// 值提供者委托。接收 <see cref="IEvaluationContext"/>，返回浮点精确值。
        /// 不可为 null。
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="provider"/> 为 null 时抛出。
        /// </exception>
        public void SetInputProvider(string variableName, Func<IEvaluationContext, float> provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            InputProvider[variableName] = provider;
        }

        /// <summary>
        /// 设置输出映射器。将解模糊化后的精确值转换为 <see cref="IActionCommand"/>。
        /// </summary>
        /// <param name="mapper">
        /// 映射委托。接收解模糊化后的浮点精确值，返回 <see cref="IActionCommand"/>。
        /// 返回 null 时，<see cref="Evaluate"/> 将返回空列表。
        /// </param>
        public void SetOutputMapper(Func<float, IActionCommand> mapper)
        {
            OutputMapper = mapper;
        }

        #endregion

        #region IDecisionStrategy 实现

        /// <inheritdoc/>
        public void Initialize()
        {
            _initialized = true;
        }

        /// <summary>
        /// 执行模糊推理并返回决策动作。
        ///
        /// 推理流程：
        /// <list type="number">
        ///   <item>通过 <see cref="InputProvider"/> 获取各输入变量的精确值</item>
        ///   <item>调用 <see cref="FuzzyVariable.Fuzzify"/> 模糊化各输入值</item>
        ///   <item>对每条规则，计算前件隶属度的最小值作为激活度</item>
        ///   <item>以激活度裁剪后件输出集合的隶属度函数（Mamdani 蕴含）</item>
        ///   <item>对所有激活规则的后件取逐点 MAX 聚合</item>
        ///   <item>重心法（COG）解模糊化得到精确输出值</item>
        ///   <item>通过 <see cref="OutputMapper"/> 将精确输出值转换为 <see cref="IActionCommand"/></item>
        /// </list>
        /// </summary>
        /// <param name="context">评估上下文，提供 Blackboard、WorldState、Perception 访问。</param>
        /// <returns>决策产生的动作指令列表。无可执行动作时返回空数组。</returns>
        /// <exception cref="InvalidOperationException">
        /// 当 <see cref="OutputVariable"/> 未设置或 <see cref="OutputMapper"/> 未配置时抛出。
        /// </exception>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (!_initialized || !IsEnabled || context == null)
            {
                return Array.Empty<IActionCommand>();
            }

            if (OutputVariable == null)
            {
                throw new InvalidOperationException("OutputVariable is not set. Call SetOutputVariable() before Evaluate().");
            }

            if (OutputMapper == null)
            {
                throw new InvalidOperationException("OutputMapper is not set. Call SetOutputMapper() before Evaluate().");
            }

            if (OutputVariable.Sets == null || OutputVariable.Sets.Count == 0)
            {
                return Array.Empty<IActionCommand>();
            }

            // 步骤 1：从 InputProvider 获取各输入变量的精确值
            var crispInputs = new Dictionary<string, float>();
            foreach (var kvp in InputVariables)
            {
                string varName = kvp.Key;

                if (InputProvider.TryGetValue(varName, out Func<IEvaluationContext, float> provider))
                {
                    crispInputs[varName] = provider(context);
                }
                else
                {
                    // 未配置提供者的变量，默认使用值域中点
                    FuzzyVariable variable = kvp.Value;
                    crispInputs[varName] = (variable.MinValue + variable.MaxValue) * 0.5f;
                }
            }

            // 步骤 2：模糊化各输入值
            var fuzzifiedInputs = new Dictionary<string, List<float>>();
            foreach (var kvp in InputVariables)
            {
                string varName = kvp.Key;
                FuzzyVariable variable = kvp.Value;

                if (crispInputs.TryGetValue(varName, out float crispValue))
                {
                    fuzzifiedInputs[varName] = variable.Fuzzify(crispValue);
                }
                else
                {
                    fuzzifiedInputs[varName] = new List<float>();
                }
            }

            // 步骤 3：计算每条规则的激活度（前件隶属度取 min）
            var ruleActivations = new List<(FuzzyRule rule, float activation)>();
            for (int i = 0; i < Rules.Count; i++)
            {
                FuzzyRule rule = Rules[i];

                if (rule == null || rule.Antecedents == null || rule.Antecedents.Count == 0)
                {
                    continue;
                }

                float activation = 1f;
                bool antecedentValid = true;

                for (int j = 0; j < rule.Antecedents.Count; j++)
                {
                    var (varName, setIndex) = rule.Antecedents[j];

                    if (!fuzzifiedInputs.TryGetValue(varName, out List<float> memberships))
                    {
                        antecedentValid = false;
                        break;
                    }

                    if (setIndex < 0 || setIndex >= memberships.Count)
                    {
                        antecedentValid = false;
                        break;
                    }

                    float membership = memberships[setIndex];

                    // AND 连接：取最小隶属度
                    activation = Math.Min(activation, membership);
                }

                if (antecedentValid)
                {
                    ruleActivations.Add((rule, activation));
                }
            }

            // 若无规则激活，返回空列表
            if (ruleActivations.Count == 0)
            {
                return Array.Empty<IActionCommand>();
            }

            // 步骤 4-6：对输出变量值域采样，进行 MAX 聚合 + 重心法解模糊化
            float outputMin = OutputVariable.MinValue;
            float outputMax = OutputVariable.MaxValue;
            float step = (outputMax - outputMin) / DefuzzificationSamples;

            float sumXTimesMu = 0f;
            float sumMu = 0f;

            for (int sampleIndex = 0; sampleIndex <= DefuzzificationSamples; sampleIndex++)
            {
                float x = outputMin + sampleIndex * step;

                // MAX 聚合：对所有激活规则的后件裁剪隶属度取最大值
                float maxMu = 0f;
                for (int r = 0; r < ruleActivations.Count; r++)
                {
                    var (rule, activation) = ruleActivations[r];
                    var (varName, setIndex) = rule.Consequent;

                    if (setIndex < 0 || setIndex >= OutputVariable.Sets.Count)
                    {
                        continue;
                    }

                    FuzzySet consequentSet = OutputVariable.Sets[setIndex];
                    if (consequentSet == null)
                    {
                        continue;
                    }

                    // 计算输出集合在此采样点上的原始隶属度
                    float rawMu = consequentSet.ComputeMembership(x);

                    // Mamdani 蕴含：裁剪（取 min）
                    float clippedMu = Math.Min(activation, rawMu);

                    // MAX 聚合
                    maxMu = Math.Max(maxMu, clippedMu);
                }

                // 重心法累积
                sumXTimesMu += x * maxMu;
                sumMu += maxMu;
            }

            // 步骤 6：重心法计算精确输出值
            float crispOutput;
            if (sumMu <= 0f)
            {
                // 无激活度时返回值域中点
                crispOutput = (outputMin + outputMax) * 0.5f;
            }
            else
            {
                crispOutput = sumXTimesMu / sumMu;
            }

            // 步骤 7：将精确输出值映射为动作指令
            IActionCommand command = OutputMapper(crispOutput);

            if (command == null)
            {
                return Array.Empty<IActionCommand>();
            }

            return new IActionCommand[] { command };
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 清空所有输入变量、规则、输入提供者和输出映射器，并重置初始化标记。
        /// 在关卡重开或 Agent 重生时调用。
        /// </remarks>
        public void Reset()
        {
            InputVariables.Clear();
            Rules.Clear();
            InputProvider.Clear();
            OutputVariable = null;
            OutputMapper = null;
            _initialized = false;
        }

        #endregion
    }
}
