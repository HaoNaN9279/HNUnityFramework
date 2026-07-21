using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Level.Logic.AI;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;

    // ============================================================
    // GOAPWorldState — 世界状态快照
    // ============================================================

    /// <summary>
    /// 表示世界状态的快照，以键值对字典（string → bool）存储状态条件。
    /// 用作 GOAP 规划器的节点，支持克隆、相等比较和哈希运算。
    /// </summary>
    public struct GOAPWorldState : IEquatable<GOAPWorldState>
    {
        /// <summary>
        /// 状态条件字典。键为条件名称，值为条件的真/假状态。
        /// </summary>
        public Dictionary<string, bool> Conditions;

        /// <summary>
        /// 使用指定容量初始化世界状态快照。
        /// </summary>
        /// <param name="capacity">预分配的字典容量。</param>
        public GOAPWorldState(int capacity)
        {
            Conditions = new Dictionary<string, bool>(capacity);
        }

        /// <summary>
        /// 深度克隆当前世界状态，生成一个完全独立的新快照。
        /// </summary>
        /// <returns>当前状态的独立副本。</returns>
        public GOAPWorldState Clone()
        {
            GOAPWorldState clone = new GOAPWorldState(Conditions.Count);

            foreach (KeyValuePair<string, bool> kvp in Conditions)
            {
                clone.Conditions[kvp.Key] = kvp.Value;
            }

            return clone;
        }

        /// <summary>
        /// 设置一个条件值。
        /// </summary>
        /// <param name="key">条件名称。</param>
        /// <param name="value">条件值。</param>
        public void Set(string key, bool value)
        {
            Conditions[key] = value;
        }

        /// <summary>
        /// 获取一个条件值。如果条件不存在则返回 false。
        /// </summary>
        /// <param name="key">条件名称。</param>
        /// <returns>条件值，不存在时返回 false。</returns>
        public bool Get(string key)
        {
            if (Conditions.TryGetValue(key, out bool value))
            {
                return value;
            }

            return false;
        }

        /// <summary>
        /// 判断当前状态是否满足指定条件集合。
        /// </summary>
        /// <param name="conditions">要检查的条件集合。</param>
        /// <returns>所有条件都满足时返回 true。</returns>
        public bool Satisfies(Dictionary<string, bool> conditions)
        {
            if (conditions == null)
            {
                return true;
            }

            foreach (KeyValuePair<string, bool> kvp in conditions)
            {
                if (!Conditions.TryGetValue(kvp.Key, out bool currentValue) || currentValue != kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 应用效果集合到当前状态（原地修改）。
        /// 将效果中的每个键值对写入条件字典。
        /// </summary>
        /// <param name="effects">要应用的效果集合。</param>
        public void ApplyEffects(Dictionary<string, bool> effects)
        {
            if (effects == null)
            {
                return;
            }

            foreach (KeyValuePair<string, bool> kvp in effects)
            {
                Conditions[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// 计算当前状态与目标条件集合之间的不匹配数量（用于启发式函数）。
        /// </summary>
        /// <param name="goalConditions">目标条件集合。</param>
        /// <returns>不匹配的条件数量。</returns>
        public int CountMismatches(Dictionary<string, bool> goalConditions)
        {
            if (goalConditions == null || goalConditions.Count == 0)
            {
                return 0;
            }

            int mismatches = 0;

            foreach (KeyValuePair<string, bool> kvp in goalConditions)
            {
                if (!Conditions.TryGetValue(kvp.Key, out bool currentValue) || currentValue != kvp.Value)
                {
                    mismatches++;
                }
            }

            return mismatches;
        }

        /// <inheritdoc/>
        public bool Equals(GOAPWorldState other)
        {
            if (other.Conditions == null && Conditions == null)
            {
                return true;
            }

            if (other.Conditions == null || Conditions == null)
            {
                return false;
            }

            if (other.Conditions.Count != Conditions.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, bool> kvp in Conditions)
            {
                if (!other.Conditions.TryGetValue(kvp.Key, out bool otherValue) || otherValue != kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is GOAPWorldState other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Conditions == null || Conditions.Count == 0)
            {
                return 0;
            }

            int hash = 17;

            // 按键排序以保证哈希一致性
            List<string> sortedKeys = new List<string>(Conditions.Keys);
            sortedKeys.Sort();

            for (int i = 0; i < sortedKeys.Count; i++)
            {
                string key = sortedKeys[i];
                hash = hash * 31 + key.GetHashCode();
                hash = hash * 31 + Conditions[key].GetHashCode();
            }

            return hash;
        }
    }

    // ============================================================
    // GOAPGoal — 目标定义
    // ============================================================

    /// <summary>
    /// GOAP 目标，定义 Agent 期望达成的世界状态。
    /// 规划器按优先级从高到低尝试为每个目标搜索可执行的动作序列。
    /// </summary>
    public class GOAPGoal
    {
        /// <summary>
        /// 目标名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 目标条件集合。Agent 期望达到的世界状态子集（键为条件名称，值为期望的真/假值）。
        /// 规划器找到的动作序列执行后，世界状态应满足所有目标条件。
        /// </summary>
        public Dictionary<string, bool> GoalConditions { get; set; }

        /// <summary>
        /// 目标优先级。数值越高，规划器越优先尝试满足此目标。
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 初始化一个空目标。
        /// </summary>
        public GOAPGoal()
        {
            Name = string.Empty;
            GoalConditions = new Dictionary<string, bool>();
        }
    }

    // ============================================================
    // GOAPAction — 动作定义
    // ============================================================

    /// <summary>
    /// GOAP 动作，定义规划器中的一个原子操作。
    /// 动作具有前置条件（执行前必须满足的状态）和效果（执行后对状态的修改），
    /// 以及用于生成实际 <see cref="IActionCommand"/> 的工厂委托。
    /// </summary>
    public class GOAPAction : IReference
    {
        /// <summary>
        /// 动作名称，用于调试和日志追踪。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 动作执行成本。规划器在多个可行路径中选择总成本最低的方案。
        /// 成本应为非负数，建议范围 0~10。
        /// </summary>
        public float Cost { get; set; } = 1f;

        /// <summary>
        /// 前置条件集合。执行此动作前必须满足的世界状态条件。
        /// 键为条件名称，值为期望的真/假值。
        /// </summary>
        public Dictionary<string, bool> Preconditions { get; set; }

        /// <summary>
        /// 效果集合。执行此动作后对世界状态的修改。
        /// 键为条件名称，值为修改后的真/假值。
        /// </summary>
        public Dictionary<string, bool> Effects { get; set; }

        /// <summary>
        /// 动作工厂委托。规划成功后，由策略调用此委托生成实际的 <see cref="IActionCommand"/> 实例。
        /// 接收 <see cref="IEvaluationContext"/>，返回 <see cref="IActionCommand"/>。
        /// </summary>
        public Func<IEvaluationContext, IActionCommand> ActionFactory { get; set; }

        /// <summary>
        /// 初始化一个空动作。
        /// </summary>
        public GOAPAction()
        {
            Name = string.Empty;
            Preconditions = new Dictionary<string, bool>();
            Effects = new Dictionary<string, bool>();
        }

        /// <summary>
        /// 判断此动作的前置条件在当前世界状态下是否满足。
        /// </summary>
        /// <param name="state">当前世界状态快照。</param>
        /// <returns>所有前置条件都满足时返回 true。</returns>
        public bool IsApplicable(GOAPWorldState state)
        {
            return state.Satisfies(Preconditions);
        }

        /// <summary>
        /// 清理动作的数据，重置到默认状态以便回收到引用池。
        /// </summary>
        public void Clear()
        {
            Name = string.Empty;
            Cost = 1f;
            Preconditions?.Clear();
            Effects?.Clear();
            ActionFactory = null;
        }
    }

    // ============================================================
    // GOAPPlanner — A* 规划器
    // ============================================================

    /// <summary>
    /// GOAP 规划器，使用 A* 搜索算法在可用动作集合中寻找从起始状态到达目标状态的最优动作序列。
    /// 启发式函数：当前状态与目标条件的不匹配数量。
    /// </summary>
    public static class GOAPPlanner
    {
        /// <summary>
        /// A* 搜索规划节点，记录搜索过程中的状态、路径和成本。
        /// </summary>
        private class PlanNode
        {
            /// <summary>
            /// 当前世界状态。
            /// </summary>
            public GOAPWorldState State;

            /// <summary>
            /// 到达当前状态已执行的动作列表。
            /// </summary>
            public List<GOAPAction> Actions;

            /// <summary>
            /// 已消耗的累积成本 g(n)。
            /// </summary>
            public float CostSoFar;

            /// <summary>
            /// 预估总成本 f(n) = g(n) + h(n)。
            /// </summary>
            public float EstimatedTotalCost;
        }

        /// <summary>
        /// PlanNode 的 f(n) 比较器，用于优先队列按预估总成本升序排列。
        /// </summary>
        private class PlanNodeComparer : IComparer<PlanNode>
        {
            public int Compare(PlanNode a, PlanNode b)
            {
                if (a == null && b == null)
                {
                    return 0;
                }

                if (a == null)
                {
                    return -1;
                }

                if (b == null)
                {
                    return 1;
                }

                int cmp = a.EstimatedTotalCost.CompareTo(b.EstimatedTotalCost);

                if (cmp != 0)
                {
                    return cmp;
                }

                return a.CostSoFar.CompareTo(b.CostSoFar);
            }
        }

        /// <summary>
        /// 使用 A* 搜索从起始状态到达目标状态的最优动作序列。
        /// </summary>
        /// <param name="startState">起始世界状态。</param>
        /// <param name="goal">目标定义，包含期望达成的条件集合。</param>
        /// <param name="availableActions">规划可用的动作列表。</param>
        /// <returns>
        /// 从起始状态到达目标状态的最优动作序列。如果无法规划则返回空列表。
        /// 返回的列表按执行顺序排列（先执行在前）。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="goal"/> 或 <paramref name="availableActions"/> 为 null 时抛出。
        /// </exception>
        public static List<GOAPAction> AStarSearch(GOAPWorldState startState, GOAPGoal goal, List<GOAPAction> availableActions)
        {
            if (goal == null)
            {
                throw new ArgumentNullException(nameof(goal));
            }

            if (availableActions == null)
            {
                throw new ArgumentNullException(nameof(availableActions));
            }

            if (availableActions.Count == 0)
            {
                return new List<GOAPAction>();
            }

            Dictionary<string, bool> goalConditions = goal.GoalConditions;

            // 快速退出：起始状态已满足目标
            if (startState.Satisfies(goalConditions))
            {
                return new List<GOAPAction>();
            }

            // OPEN 优先队列，按 f(n) = g(n) + h(n) 升序
            PlanNodeComparer comparer = new PlanNodeComparer();
            List<PlanNode> openList = new List<PlanNode>();

            // CLOSED 集：已探索的状态哈希值集合
            HashSet<int> closedSet = new HashSet<int>();

            // 起始节点
            PlanNode startNode = new PlanNode
            {
                State = startState,
                Actions = new List<GOAPAction>(),
                CostSoFar = 0f,
                EstimatedTotalCost = startState.CountMismatches(goalConditions)
            };

            openList.Add(startNode);
            closedSet.Add(startState.GetHashCode());

            while (openList.Count > 0)
            {
                // 取出 f(n) 最小的节点
                PlanNode current = openList[0];
                openList.RemoveAt(0);

                // 检查是否达到目标
                if (current.State.Satisfies(goalConditions))
                {
                    return current.Actions;
                }

                // 展开当前节点的所有可行动作
                for (int i = 0; i < availableActions.Count; i++)
                {
                    GOAPAction action = availableActions[i];

                    if (action == null)
                    {
                        continue;
                    }

                    // 检查前置条件是否满足
                    if (!action.IsApplicable(current.State))
                    {
                        continue;
                    }

                    // 计算后继状态
                    GOAPWorldState successorState = current.State.Clone();
                    successorState.ApplyEffects(action.Effects);

                    int successorHash = successorState.GetHashCode();

                    // 检查是否已在 CLOSED 集中
                    if (closedSet.Contains(successorHash))
                    {
                        continue;
                    }

                    closedSet.Add(successorHash);

                    // 构建后继节点
                    List<GOAPAction> newActions = new List<GOAPAction>(current.Actions.Count + 1);
                    newActions.AddRange(current.Actions);
                    newActions.Add(action);

                    float newCost = current.CostSoFar + action.Cost;
                    float heuristic = successorState.CountMismatches(goalConditions);

                    PlanNode successorNode = new PlanNode
                    {
                        State = successorState,
                        Actions = newActions,
                        CostSoFar = newCost,
                        EstimatedTotalCost = newCost + heuristic
                    };

                    // 按 f(n) 升序插入 OPEN 列表
                    InsertSorted(openList, successorNode, comparer);
                }
            }

            // 无可达路径
            return new List<GOAPAction>();
        }

        /// <summary>
        /// 在已排序的列表中按比较器规则插入元素，保持升序。
        /// </summary>
        /// <param name="list">目标列表。</param>
        /// <param name="item">要插入的元素。</param>
        /// <param name="comparer">比较器。</param>
        private static void InsertSorted(List<PlanNode> list, PlanNode item, IComparer<PlanNode> comparer)
        {
            int index = list.BinarySearch(item, comparer);

            if (index < 0)
            {
                index = ~index;
            }

            list.Insert(index, item);
        }
    }

    // ============================================================
    // GOAPStrategy — 目标导向行动规划策略
    // ============================================================

    /// <summary>
    /// 目标导向行动规划策略（GOAP）— 基于 A* 搜索的 AI 决策器。
    /// </summary>
    /// <remarks>
    /// <para>
    /// GOAP（Goal-Oriented Action Planning）通过逆向搜索从起始世界状态到目标状态的最优动作序列。
    /// 每次 <see cref="Evaluate"/> 调用时：
    /// </para>
    /// <list type="number">
    /// <item>从 <see cref="IEvaluationContext.WorldState"/> 构建当前世界状态快照。</item>
    /// <item>按优先级降序排列 <see cref="Goals"/>，逐一尝试规划。</item>
    /// <item>对每个目标，调用 <see cref="GOAPPlanner.AStarSearch"/> 搜索动作序列。</item>
    /// <item>规划成功时执行返回的动作序列，每个动作通过其 <see cref="GOAPAction.ActionFactory"/> 生成 <see cref="IActionCommand"/>。</item>
    /// </list>
    /// <para>
    /// 使用示例：
    /// </para>
    /// <code>
    /// var strategy = new GOAPStrategy();
    ///
    /// // 定义目标
    /// var killEnemyGoal = new GOAPGoal
    /// {
    ///     Name = "KillEnemy",
    ///     Priority = 10,
    ///     GoalConditions = new Dictionary&lt;string, bool&gt;
    ///     {
    ///         { "IsEnemyDead", true }
    ///     }
    /// };
    ///
    /// var collectItemGoal = new GOAPGoal
    /// {
    ///     Name = "CollectItem",
    ///     Priority = 5,
    ///     GoalConditions = new Dictionary&lt;string, bool&gt;
    ///     {
    ///         { "HasItem", true }
    ///     }
    /// };
    ///
    /// // 定义动作
    /// var attackAction = new GOAPAction
    /// {
    ///     Name = "Attack",
    ///     Cost = 2f,
    ///     Preconditions = new Dictionary&lt;string, bool&gt; { { "HasWeapon", true } },
    ///     Effects = new Dictionary&lt;string, bool&gt; { { "IsEnemyDead", true } },
    ///     ActionFactory = ctx => ReferencePool.Acquire&lt;YourAttackAction&gt;()
    /// };
    ///
    /// var equipWeaponAction = new GOAPAction
    /// {
    ///     Name = "EquipWeapon",
    ///     Cost = 1f,
    ///     Preconditions = new Dictionary&lt;string, bool&gt; { { "HasWeaponInInventory", true } },
    ///     Effects = new Dictionary&lt;string, bool&gt; { { "HasWeapon", true } },
    ///     ActionFactory = ctx => ReferencePool.Acquire&lt;YourEquipAction&gt;()
    /// };
    ///
    /// strategy.AddGoal(killEnemyGoal);
    /// strategy.AddGoal(collectItemGoal);
    /// strategy.AddAction(attackAction);
    /// strategy.AddAction(equipWeaponAction);
    /// strategy.Initialize();
    ///
    /// // 每帧评估
    /// var commands = strategy.Evaluate(context);
    /// </code>
    /// </remarks>
    public class GOAPStrategy : IDecisionStrategy
    {
        private List<GOAPGoal> _goals;
        private List<GOAPAction> _actions;
        private bool _initialized;

        // 已规划的动作序列（跨帧执行）
        private List<GOAPAction> _currentPlan;
        private int _currentPlanIndex;

        /// <inheritdoc/>
        public string Name => "GOAP";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 获取当前已注册的目标列表（只读）。
        /// </summary>
        public IReadOnlyList<GOAPGoal> Goals => _goals;

        /// <summary>
        /// 获取当前已注册的动作列表（只读）。
        /// </summary>
        public IReadOnlyList<GOAPAction> Actions => _actions;

        /// <summary>
        /// 当前正在执行的动作序列（只读）。已执行完毕的动作不在此列表中。
        /// </summary>
        public IReadOnlyList<GOAPAction> CurrentPlan => _currentPlan;

        /// <summary>
        /// 初始化一个空的 GOAP 策略。
        /// </summary>
        public GOAPStrategy()
        {
            _goals = new List<GOAPGoal>();
            _actions = new List<GOAPAction>();
            _currentPlan = new List<GOAPAction>();
            _currentPlanIndex = 0;
        }

        #region 目标管理

        /// <summary>
        /// 添加一个目标到策略中。
        /// </summary>
        /// <param name="goal">要添加的 GOAP 目标。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="goal"/> 为 null 时抛出。</exception>
        public void AddGoal(GOAPGoal goal)
        {
            if (goal == null)
            {
                throw new ArgumentNullException(nameof(goal));
            }

            _goals.Add(goal);
        }

        /// <summary>
        /// 移除一个目标。
        /// </summary>
        /// <param name="goal">要移除的目标。</param>
        /// <returns>成功移除返回 true，若不存在返回 false。</returns>
        public bool RemoveGoal(GOAPGoal goal)
        {
            return _goals.Remove(goal);
        }

        /// <summary>
        /// 按名称移除目标。
        /// </summary>
        /// <param name="name">目标名称。</param>
        /// <returns>成功移除返回 true，若不存在返回 false。</returns>
        public bool RemoveGoalByName(string name)
        {
            for (int i = _goals.Count - 1; i >= 0; i--)
            {
                if (_goals[i].Name == name)
                {
                    _goals.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清空所有目标。
        /// </summary>
        public void ClearGoals()
        {
            _goals.Clear();
        }

        #endregion

        #region 动作管理

        /// <summary>
        /// 添加一个动作到策略中。
        /// </summary>
        /// <param name="action">要添加的 GOAP 动作。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="action"/> 为 null 时抛出。</exception>
        public void AddAction(GOAPAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _actions.Add(action);
        }

        /// <summary>
        /// 移除一个动作。
        /// </summary>
        /// <param name="action">要移除的动作。</param>
        /// <returns>成功移除返回 true，若不存在返回 false。</returns>
        public bool RemoveAction(GOAPAction action)
        {
            return _actions.Remove(action);
        }

        /// <summary>
        /// 按名称移除动作。
        /// </summary>
        /// <param name="name">动作名称。</param>
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
        /// 清空所有动作。
        /// </summary>
        public void ClearActions()
        {
            _actions.Clear();
        }

        #endregion

        /// <inheritdoc/>
        public void Initialize()
        {
            _initialized = true;
            _currentPlan.Clear();
            _currentPlanIndex = 0;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (!_initialized || !IsEnabled || context == null)
            {
                return Array.Empty<IActionCommand>();
            }

            if (_goals.Count == 0 || _actions.Count == 0)
            {
                return Array.Empty<IActionCommand>();
            }

            // 如果当前有正在执行的计划，继续推进
            if (_currentPlan.Count > 0 && _currentPlanIndex < _currentPlan.Count)
            {
                GOAPAction nextAction = _currentPlan[_currentPlanIndex];
                _currentPlanIndex++;

                if (nextAction.ActionFactory != null)
                {
                    IActionCommand command = nextAction.ActionFactory(context);

                    if (command != null)
                    {
                        return new IActionCommand[] { command };
                    }
                }
            }

            // 无进行中的计划或计划已执行完毕，尝试重新规划
            return Replan(context);
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _goals.Clear();
            _actions.Clear();
            _currentPlan.Clear();
            _currentPlanIndex = 0;
            _initialized = false;
        }

        /// <summary>
        /// 强制重新规划。清空当前计划并立即尝试规划新的动作序列。
        /// 在外部条件发生重大变化（如目标达成、计划中断）时调用。
        /// </summary>
        public void ForceReplan()
        {
            _currentPlan.Clear();
            _currentPlanIndex = 0;
        }

        #region 规划逻辑

        /// <summary>
        /// 重新规划：从当前世界状态出发，按优先级尝试规划目标并执行。
        /// </summary>
        /// <param name="context">评估上下文。</param>
        /// <returns>规划成功时返回第一个要执行的动作指令。</returns>
        private IReadOnlyList<IActionCommand> Replan(IEvaluationContext context)
        {
            // 1. 构建当前世界状态快照
            GOAPWorldState currentState = BuildWorldState(context);

            // 2. 按优先级降序排序目标
            List<GOAPGoal> sortedGoals = GetSortedGoals();

            // 3. 逐一尝试规划
            for (int i = 0; i < sortedGoals.Count; i++)
            {
                GOAPGoal goal = sortedGoals[i];

                // 快速退出：当前状态已满足目标
                if (currentState.Satisfies(goal.GoalConditions))
                {
                    continue;
                }

                List<GOAPAction> plan = GOAPPlanner.AStarSearch(currentState, goal, _actions);

                if (plan.Count > 0)
                {
                    // 保存计划并执行第一个动作
                    _currentPlan.Clear();
                    _currentPlan.AddRange(plan);
                    _currentPlanIndex = 0;

                    // 执行第一个动作
                    if (_currentPlan.Count > 0)
                    {
                        GOAPAction firstAction = _currentPlan[0];
                        _currentPlanIndex = 1;

                        if (firstAction.ActionFactory != null)
                        {
                            IActionCommand command = firstAction.ActionFactory(context);

                            if (command != null)
                            {
                                return new IActionCommand[] { command };
                            }
                        }
                    }

                    break;
                }
            }

            return Array.Empty<IActionCommand>();
        }

        /// <summary>
        /// 从评估上下文中构建当前世界状态快照。
        /// 将 <see cref="IEvaluationContext.WorldState"/> 和 <see cref="IEvaluationContext.Blackboard"/> 中的布尔键值转换为 GOAP 可用状态。
        /// </summary>
        /// <param name="context">评估上下文。</param>
        /// <returns>当前世界状态的 GOAP 快照。</returns>
        private GOAPWorldState BuildWorldState(IEvaluationContext context)
        {
            GOAPWorldState state = new GOAPWorldState(16);

            // 从 WorldStateCache 中提取布尔值
            if (context.WorldState != null)
            {
                // 收集所有规划动作涉及的条件名称
                HashSet<string> relevantKeys = CollectRelevantKeys();
                IEnumerator<string> enumerator = relevantKeys.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current;

                    if (context.WorldState.HasKey(key))
                    {
                        if (context.WorldState.TryGet<bool>(key, out bool value))
                        {
                            state.Set(key, value);
                        }
                    }
                }
            }

            // 从 Blackboard 中提取布尔值
            if (context.Blackboard != null)
            {
                HashSet<string> relevantKeys = CollectRelevantKeys();
                IEnumerator<string> enumerator = relevantKeys.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current;

                    if (context.Blackboard.HasKey(key))
                    {
                        if (context.Blackboard.TryGet<bool>(key, out bool value))
                        {
                            // 不覆盖 WorldState 中已有的值
                            if (!state.Conditions.ContainsKey(key))
                            {
                                state.Set(key, value);
                            }
                        }
                    }
                }
            }

            return state;
        }

        /// <summary>
        /// 收集所有目标和动作中用到的条件键，避免无差别遍历所有状态。
        /// </summary>
        /// <returns>所有相关条件键的集合。</returns>
        private HashSet<string> CollectRelevantKeys()
        {
            HashSet<string> keys = new HashSet<string>();

            for (int i = 0; i < _goals.Count; i++)
            {
                GOAPGoal goal = _goals[i];

                if (goal.GoalConditions != null)
                {
                    foreach (string key in goal.GoalConditions.Keys)
                    {
                        keys.Add(key);
                    }
                }
            }

            for (int i = 0; i < _actions.Count; i++)
            {
                GOAPAction action = _actions[i];

                if (action.Preconditions != null)
                {
                    foreach (string key in action.Preconditions.Keys)
                    {
                        keys.Add(key);
                    }
                }

                if (action.Effects != null)
                {
                    foreach (string key in action.Effects.Keys)
                    {
                        keys.Add(key);
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// 获取按优先级降序排列的目标列表。
        /// </summary>
        /// <returns>排序后的目标列表（新列表，不影响原始数据）。</returns>
        private List<GOAPGoal> GetSortedGoals()
        {
            List<GOAPGoal> sorted = new List<GOAPGoal>(_goals);
            sorted.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            return sorted;
        }

        #endregion
    }
}
