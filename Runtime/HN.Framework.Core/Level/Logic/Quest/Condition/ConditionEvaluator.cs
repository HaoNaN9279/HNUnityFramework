#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 条件评估器。管理运行时条件状态，评估 Counter/State/MultiCounter 条件是否满足，
    /// 并在条件满足时触发 <see cref="OnConditionMet"/> 事件。
    /// 由 <see cref="QuestManager{TId}"/> 驱动，不对外暴露。
    /// </summary>
    /// <typeparam name="TId">实体标识类型，必须可判等。</typeparam>
    public sealed class ConditionEvaluator<TId> where TId : IEquatable<TId>
    {
        private readonly Func<string, string, bool>? _stateChecker;

        // 条件配置
        private readonly Dictionary<int, ConditionDef> _conditionDefs = new();
        private readonly Dictionary<int, ConditionGroupDef> _conditionGroups = new();

        // questHandleId → 条件组 ID 列表
        private readonly Dictionary<int, List<int>> _questConditionGroups = new();
        // conditionId → questHandleId（反向索引）
        private readonly Dictionary<int, int> _conditionToQuest = new();

        // Counter 条件运行时状态（conditionId → CounterState）
        private readonly Dictionary<int, CounterState> _counterStates = new();

        // eventTypeName → 监听该事件的 Counter 条件 ID 列表
        private readonly Dictionary<string, List<int>> _eventCounters = new();

        /// <summary>
        /// 条件满足事件（conditionId, questHandleId）。
        /// </summary>
        public event Action<int, int>? OnConditionMet;

        /// <summary>
        /// 初始化条件评估器。
        /// </summary>
        /// <param name="stateChecker">状态检查委托，用于评估 State 类型条件。key 为状态键名，value 为预期值。</param>
        public ConditionEvaluator(Func<string, string, bool>? stateChecker = null)
        {
            _stateChecker = stateChecker;
        }

        /// <summary>
        /// 加载条件配置数据。
        /// </summary>
        /// <param name="conditionDefs">条件定义列表。</param>
        /// <param name="conditionGroups">条件组定义列表。</param>
        public void LoadConfigs(IEnumerable<ConditionDef> conditionDefs, IEnumerable<ConditionGroupDef> conditionGroups)
        {
            foreach (var cd in conditionDefs)
            {
                _conditionDefs[cd.Id] = cd;
            }

            foreach (var cg in conditionGroups)
            {
                _conditionGroups[cg.Id] = cg;
            }
        }

        /// <summary>
        /// 为指定任务注册需要追踪的条件组。
        /// 内部解析每个条件组中的 Counter 条件，构建事件监听映射。
        /// </summary>
        /// <param name="questHandleId">任务句柄的内部 ID。</param>
        /// <param name="conditionGroupIds">条件组 ID 列表。</param>
        public void RegisterQuestConditions(int questHandleId, IReadOnlyList<int> conditionGroupIds)
        {
            _questConditionGroups[questHandleId] = new List<int>(conditionGroupIds);

            foreach (var groupId in conditionGroupIds)
            {
                if (!_conditionGroups.TryGetValue(groupId, out var group) || group.ConditionIds == null)
                {
                    continue;
                }

                foreach (var conditionId in group.ConditionIds)
                {
                    _conditionToQuest[conditionId] = questHandleId;

                    if (!_conditionDefs.TryGetValue(conditionId, out var def))
                    {
                        continue;
                    }

                    // Counter 条件：记录事件类型 → 条件 ID 映射
                    if (def.Type == ConditionType.Counter)
                    {
                        var eventType = def.GetParameter("EventTypeName");
                        if (!string.IsNullOrEmpty(eventType))
                        {
                            if (!_eventCounters.ContainsKey(eventType))
                            {
                                _eventCounters[eventType] = new List<int>();
                            }

                            _eventCounters[eventType].Add(conditionId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通知游戏事件发生，驱动 Counter 条件计数递增。
        /// </summary>
        /// <param name="eventTypeName">事件类型名称，对应 ConditionDef.Parameters["EventTypeName"]。</param>
        /// <param name="ownerId">事件关联的实体 ID（可选，用于 State 条件评估）。</param>
        public void NotifyEvent(string eventTypeName, TId ownerId = default)
        {
            if (!_eventCounters.TryGetValue(eventTypeName, out var conditionIds))
            {
                return;
            }

            foreach (var conditionId in conditionIds)
            {
                if (!_conditionToQuest.TryGetValue(conditionId, out var questHandleId))
                {
                    continue;
                }

                if (!_conditionDefs.TryGetValue(conditionId, out var def))
                {
                    continue;
                }

                int targetCount = int.TryParse(def.GetParameter("TargetCount"), out var tc) ? tc : 1;

                if (!_counterStates.TryGetValue(conditionId, out var state))
                {
                    state = new CounterState
                    {
                        ConditionId = conditionId,
                        CurrentCount = 1,
                        TargetCount = targetCount,
                    };
                    _counterStates[conditionId] = state;
                }
                else if (!state.IsCompleted)
                {
                    state.CurrentCount++;
                }

                if (state.CurrentCount >= state.TargetCount && !state.IsCompleted)
                {
                    state.IsCompleted = true;
                    OnConditionMet?.Invoke(conditionId, questHandleId);
                }
            }
        }

        /// <summary>
        /// 检查某任务的所有条件组是否全部满足。
        /// </summary>
        /// <param name="questHandleId">任务句柄的内部 ID。</param>
        /// <returns>全部满足返回 true。</returns>
        public bool AreAllGroupsMet(int questHandleId)
        {
            if (!_questConditionGroups.TryGetValue(questHandleId, out var groupIds) || groupIds.Count == 0)
            {
                return true;
            }

            foreach (var groupId in groupIds)
            {
                if (!IsGroupMet(groupId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查单个条件组是否满足（组内所有条件 AND 关系）。
        /// </summary>
        private bool IsGroupMet(int groupId)
        {
            if (!_conditionGroups.TryGetValue(groupId, out var group) || group.ConditionIds == null || group.ConditionIds.Count == 0)
            {
                return true;
            }

            foreach (var conditionId in group.ConditionIds)
            {
                if (!IsConditionMet(conditionId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查单个条件是否满足。
        /// Counter 类型检查 CounterState.IsCompleted；
        /// State 类型通过传入的 stateChecker 委托评估。
        /// </summary>
        private bool IsConditionMet(int conditionId)
        {
            if (_counterStates.TryGetValue(conditionId, out var state))
            {
                return state.IsCompleted;
            }

            if (_conditionDefs.TryGetValue(conditionId, out var def))
            {
                if (def.Type == ConditionType.State && _stateChecker != null)
                {
                    var stateKey = def.GetParameter("StateKey");
                    var expectedValue = def.GetParameter("ExpectedValue");
                    return _stateChecker(stateKey, expectedValue);
                }
            }

            return false;
        }
    }
}
