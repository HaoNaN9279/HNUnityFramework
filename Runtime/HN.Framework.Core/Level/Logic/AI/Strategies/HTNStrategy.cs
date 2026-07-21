namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    using System;
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Level.Logic.AI;

    /// <summary>
    /// HTN 任务类型枚举。
    /// 区分叶子节点（原始任务）和内部节点（复合任务）。
    /// </summary>
    public enum HTNTaskType
    {
        /// <summary>
        /// 原始任务 — 可直接执行的原子操作，拥有 <see cref="HTNTask.ActionFactory"/>。
        /// </summary>
        Primitive,

        /// <summary>
        /// 复合任务 — 需通过 <see cref="HTNMethod"/> 分解为若干子任务。
        /// </summary>
        Compound
    }

    /// <summary>
    /// HTN 任务节点 — 层次任务网络中的基本单元。
    /// 可以是可执行的原子操作（原始任务），也可以是需要进一步分解的抽象目标（复合任务）。
    /// 实现 <see cref="IReference"/>，支持对象池管理。
    /// </summary>
    public class HTNTask : IReference
    {
        /// <summary>
        /// 任务名称，用于在 <see cref="HTNDomain"/> 中索引。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 任务类型：原始任务或复合任务。
        /// </summary>
        public HTNTaskType Type { get; set; }

        /// <summary>
        /// 动作工厂 — 仅对原始任务有效。
        /// 当此类任务被规划器分解到最终计划中时，通过此委托创建对应的 <see cref="IActionCommand"/>。
        /// </summary>
        public Func<IEvaluationContext, IActionCommand> ActionFactory { get; set; }

        /// <summary>
        /// 条件委托 — 可选的条件分支。
        /// 若不为 null，则在规划分解时检查此条件：条件不满足时跳过该任务或视为分解失败。
        /// </summary>
        public Func<IEvaluationContext, bool> Condition { get; set; }

        /// <summary>
        /// 子任务列表 — 仅对复合任务有效，定义此复合任务的直接分解方案。
        /// 通常由 <see cref="HTNMethod"/> 在注册时填充，也可直接配置。
        /// </summary>
        public List<HTNTask> Subtasks { get; set; }

        /// <summary>
        /// 初始化一个默认的原始任务。
        /// </summary>
        public HTNTask()
        {
            Name = string.Empty;
            Type = HTNTaskType.Primitive;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 将所有属性重置为默认值，便于对象池回收复用。
        /// </remarks>
        public void Clear()
        {
            Name = string.Empty;
            Type = HTNTaskType.Primitive;
            ActionFactory = null;
            Condition = null;
            Subtasks = null;
        }
    }

    /// <summary>
    /// HTN 方法 — 定义复合任务的候选分解方式。
    /// 一个复合任务可以注册多个 <see cref="HTNMethod"/>，每个方法包含一组子任务和一个前置条件。
    /// 规划器按注册顺序依次尝试方法，选择第一个条件满足的方法进行分解。
    /// </summary>
    public class HTNMethod
    {
        /// <summary>
        /// 方法名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 该方法的子任务列表，定义复合任务的一种具体实现路径。
        /// </summary>
        public List<HTNTask> Subtasks { get; set; }

        /// <summary>
        /// 前置条件 — 可选。
        /// 若不为 null，仅当此条件在当前上下文中返回 true 时，该方法才被视为可用。
        /// </summary>
        public Func<IEvaluationContext, bool> Condition { get; set; }

        /// <summary>
        /// 初始化一个默认的 HTN 方法。
        /// </summary>
        public HTNMethod()
        {
            Name = string.Empty;
            Subtasks = new List<HTNTask>();
        }
    }

    /// <summary>
    /// HTN 领域 — 管理所有任务定义和复合任务的分解方法。
    /// 作为 <see cref="HTNStrategy"/> 的核心数据模型，存储完整的任务层次网络。
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// var domain = new HTNDomain();
    /// // 注册原始任务
    /// domain.RegisterTask(new HTNTask
    /// {
    ///     Name = "Attack",
    ///     Type = HTNTaskType.Primitive,
    ///     ActionFactory = ctx => new AttackCommand()
    /// });
    /// // 注册复合任务
    /// domain.RegisterTask(new HTNTask
    /// {
    ///     Name = "Combat",
    ///     Type = HTNTaskType.Compound
    /// });
    /// // 为复合任务注册分解方法
    /// domain.RegisterMethod("Combat", new HTNMethod
    /// {
    ///     Name = "MeleeCombat",
    ///     Condition = ctx => ctx.Perception.NearestEnemyDistance &lt; 5f,
    ///     Subtasks = new List&lt;HTNTask&gt; { /* ... */ }
    /// });
    /// </code>
    /// </remarks>
    public class HTNDomain
    {
        private readonly Dictionary<string, HTNTask> _tasks;
        private readonly Dictionary<string, List<HTNMethod>> _methods;

        /// <summary>
        /// 初始化一个空的 HTN 领域。
        /// </summary>
        public HTNDomain()
        {
            _tasks = new Dictionary<string, HTNTask>();
            _methods = new Dictionary<string, List<HTNMethod>>();
        }

        /// <summary>
        /// 获取已注册的任务字典（只读）。
        /// </summary>
        public IReadOnlyDictionary<string, HTNTask> Tasks => _tasks;

        /// <summary>
        /// 获取已注册的方法字典（只读）。
        /// Key 为复合任务名称，Value 为该任务对应的分解方法列表。
        /// </summary>
        public IReadOnlyDictionary<string, List<HTNMethod>> Methods => _methods;

        /// <summary>
        /// 注册一个任务。任务名称在领域内必须唯一。
        /// 若同名任务已存在则覆盖。
        /// </summary>
        /// <param name="task">要注册的任务。不可为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="task"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="task"/> 的 <see cref="HTNTask.Name"/> 为 null 或空时抛出。</exception>
        public void RegisterTask(HTNTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (string.IsNullOrEmpty(task.Name))
            {
                throw new ArgumentException("Task name must not be null or empty.", nameof(task));
            }

            _tasks[task.Name] = task;
        }

        /// <summary>
        /// 为指定复合任务注册一个分解方法。
        /// 方法的注册顺序即为规划器的尝试优先级——先注册的方法优先匹配。
        /// </summary>
        /// <param name="taskName">复合任务名称，必须在 <see cref="Tasks"/> 中已注册。</param>
        /// <param name="method">分解方法。不可为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="method"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="taskName"/> 为 null 或空时抛出。</exception>
        public void RegisterMethod(string taskName, HTNMethod method)
        {
            if (string.IsNullOrEmpty(taskName))
            {
                throw new ArgumentException("Task name must not be null or empty.", nameof(taskName));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (!_methods.TryGetValue(taskName, out List<HTNMethod> methodList))
            {
                methodList = new List<HTNMethod>();
                _methods[taskName] = methodList;
            }

            methodList.Add(method);
        }
    }

    /// <summary>
    /// HTN 规划器 — 自顶向下递归分解层次任务网络。
    /// 从根任务出发，将复合任务通过匹配的 <see cref="HTNMethod"/> 逐步分解为原始任务序列。
    /// </summary>
    /// <remarks>
    /// 规划算法采用深度优先、方法优先匹配的策略：
    /// <list type="number">
    ///   <item>若当前任务为原始任务，直接加入结果列表</item>
    ///   <item>若当前任务为复合任务，按注册顺序尝试所有已注册方法</item>
    ///   <item>选择第一个前置条件满足的方法，递归分解其子任务</item>
    ///   <item>若所有方法均不满足条件，则规划失败返回 null</item>
    /// </list>
    /// </remarks>
    public static class HTNPlanner
    {
        /// <summary>
        /// 执行 HTN 规划，返回从根任务完全分解后的原始任务序列。
        /// </summary>
        /// <param name="domain">HTN 领域，包含所有任务和方法定义。不可为 null。</param>
        /// <param name="rootTaskName">根任务名称，规划从此任务开始。不可为 null 或空。</param>
        /// <param name="context">当前 AI 评估上下文，用于条件评估。</param>
        /// <returns>
        /// 分解后的原始任务列表。若规划成功但最终无任何原始任务则返回空列表。
        /// 若规划失败（根任务不存在、无可用方法等）则返回 null。
        /// </returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="domain"/> 或 <paramref name="context"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="rootTaskName"/> 为 null 或空时抛出。</exception>
        public static List<HTNTask> Plan(HTNDomain domain, string rootTaskName, IEvaluationContext context)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            if (string.IsNullOrEmpty(rootTaskName))
            {
                throw new ArgumentException("Root task name must not be null or empty.", nameof(rootTaskName));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!domain.Tasks.TryGetValue(rootTaskName, out HTNTask rootTask))
            {
                return null;
            }

            var result = new List<HTNTask>();

            if (!Decompose(domain, rootTask, context, result))
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// 递归分解单个 HTN 任务。
        /// </summary>
        /// <param name="domain">HTN 领域。</param>
        /// <param name="task">当前待分解的任务。</param>
        /// <param name="context">评估上下文。</param>
        /// <param name="result">累积的原始任务结果列表。</param>
        /// <returns>分解成功返回 true，失败返回 false。</returns>
        private static bool Decompose(HTNDomain domain, HTNTask task, IEvaluationContext context, List<HTNTask> result)
        {
            if (task == null)
            {
                return false;
            }

            // 检查任务级条件：不满足则跳过本任务但不视为失败
            if (task.Condition != null && !task.Condition(context))
            {
                return true;
            }

            if (task.Type == HTNTaskType.Primitive)
            {
                result.Add(task);
                return true;
            }

            // 复合任务：尝试查找可用的分解方法
            if (!domain.Methods.TryGetValue(task.Name, out List<HTNMethod> methodList) || methodList.Count == 0)
            {
                // 尝试使用任务内联的 Subtasks 作为分解方案
                if (task.Subtasks != null && task.Subtasks.Count > 0)
                {
                    for (int i = 0; i < task.Subtasks.Count; i++)
                    {
                        if (!Decompose(domain, task.Subtasks[i], context, result))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            // 按注册顺序尝试每个方法
            for (int i = 0; i < methodList.Count; i++)
            {
                HTNMethod method = methodList[i];

                if (method == null)
                {
                    continue;
                }

                // 检查方法前置条件
                if (method.Condition != null && !method.Condition(context))
                {
                    continue;
                }

                // 条件满足，递归分解此方法的子任务
                if (method.Subtasks == null || method.Subtasks.Count == 0)
                {
                    return true;
                }

                for (int j = 0; j < method.Subtasks.Count; j++)
                {
                    if (!Decompose(domain, method.Subtasks[j], context, result))
                    {
                        return false;
                    }
                }

                return true;
            }

            // 所有方法均不满足条件，分解失败
            return false;
        }
    }

    /// <summary>
    /// 层次任务网络（HTN）决策策略 — 基于自顶向下任务分解的 AI 决策器。
    /// 将复杂的 AI 行为建模为层次化的任务网络：复合任务通过方法分解为子任务，
    /// 最终分解为可执行的原始任务（原子动作）。
    /// </summary>
    /// <remarks>
    /// <para>HTN 适合表达具有明确层次结构的行为模式，例如：</para>
    /// <list type="bullet">
    ///   <item>战斗行为 → 近战攻击 / 远程攻击 → 寻路 + 开火</item>
    ///   <item>巡逻行为 → 按路径点移动 → 移动到下一个点 + 等待</item>
    ///   <item>生存行为 → 寻找掩体 / 使用治疗物品 → 检测掩体 + 移动到掩体</item>
    /// </list>
    ///
    /// <para>使用示例：</para>
    /// <code>
    /// var domain = new HTNDomain();
    ///
    /// // 注册原始任务
    /// var moveTo = new HTNTask
    /// {
    ///     Name = "MoveToEnemy",
    ///     Type = HTNTaskType.Primitive,
    ///     ActionFactory = ctx => new MoveToCommand(ctx.Perception.NearestEnemyPosition)
    /// };
    /// var attack = new HTNTask
    /// {
    ///     Name = "AttackEnemy",
    ///     Type = HTNTaskType.Primitive,
    ///     ActionFactory = ctx => new AttackCommand()
    /// };
    /// domain.RegisterTask(moveTo);
    /// domain.RegisterTask(attack);
    ///
    /// // 注册复合任务
    /// var combat = new HTNTask
    /// {
    ///     Name = "Combat",
    ///     Type = HTNTaskType.Compound
    /// };
    /// domain.RegisterTask(combat);
    ///
    /// // 为复合任务注册分解方法
    /// domain.RegisterMethod("Combat", new HTNMethod
    /// {
    ///     Name = "MeleeApproach",
    ///     Condition = ctx => ctx.Perception.NearestEnemyDistance &lt; 2f,
    ///     Subtasks = new List&lt;HTNTask&gt; { attack }
    /// });
    /// domain.RegisterMethod("Combat", new HTNMethod
    /// {
    ///     Name = "ApproachThenAttack",
    ///     Subtasks = new List&lt;HTNTask&gt; { moveTo, attack }
    /// });
    ///
    /// var strategy = new HTNStrategy
    /// {
    ///     Domain = domain,
    ///     RootTaskName = "Combat"
    /// };
    /// strategy.Initialize();
    /// var commands = strategy.Evaluate(context);
    /// </code>
    /// </remarks>
    public class HTNStrategy : IDecisionStrategy
    {
        private bool _initialized;

        /// <summary>
        /// HTN 领域，包含所有任务定义和分解方法。
        /// 在 <see cref="Initialize"/> 之前配置。
        /// </summary>
        public HTNDomain Domain { get; set; }

        /// <summary>
        /// 规划起始的根任务名称。必须存在于 <see cref="Domain"/> 的 <see cref="HTNDomain.Tasks"/> 中。
        /// </summary>
        public string RootTaskName { get; set; }

        /// <inheritdoc/>
        public string Name => "HTN";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 初始化一个空的 HTN 策略。
        /// </summary>
        public HTNStrategy()
        {
            Domain = new HTNDomain();
            RootTaskName = string.Empty;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            _initialized = true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 评估流程：
        /// <list type="number">
        ///   <li>检查策略状态：已初始化、已启用、context 非 null</li>
        ///   <li>检查 <see cref="Domain"/> 和 <see cref="RootTaskName"/> 是否已配置</li>
        ///   <li>调用 <see cref="HTNPlanner.Plan"/> 从根任务开始自顶向下递归分解</li>
        ///   <li>若规划失败（返回 null），则返回空指令列表</li>
        ///   <li>遍历分解后的原始任务序列，依次调用 <see cref="HTNTask.ActionFactory"/> 生成动作指令</li>
        /// </list>
        /// </remarks>
        /// <param name="context">AI 评估上下文，提供 Blackboard、WorldState、Perception 访问。</param>
        /// <returns>分解并生成的动作指令列表。规划失败或无条件满足时返回空列表。</returns>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (!_initialized || !IsEnabled || context == null)
            {
                return Array.Empty<IActionCommand>();
            }

            if (Domain == null || string.IsNullOrEmpty(RootTaskName))
            {
                return Array.Empty<IActionCommand>();
            }

            List<HTNTask> plan = HTNPlanner.Plan(Domain, RootTaskName, context);

            if (plan == null || plan.Count == 0)
            {
                return Array.Empty<IActionCommand>();
            }

            var commands = new List<IActionCommand>(plan.Count);

            for (int i = 0; i < plan.Count; i++)
            {
                HTNTask task = plan[i];

                if (task == null || task.ActionFactory == null)
                {
                    continue;
                }

                IActionCommand command = task.ActionFactory(context);

                if (command != null)
                {
                    commands.Add(command);
                }
            }

            return commands;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 重置初始化标记并清空领域数据。
        /// 在关卡重开或 Agent 重生后调用，允许重新配置领域并初始化。
        /// </remarks>
        public void Reset()
        {
            _initialized = false;
            Domain = new HTNDomain();
            RootTaskName = string.Empty;
        }
    }
}
