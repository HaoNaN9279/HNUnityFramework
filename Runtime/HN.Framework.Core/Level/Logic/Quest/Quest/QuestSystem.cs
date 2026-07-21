#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务系统实现。管理任务的生命周期，包括接受、完成、领取奖励、放弃和 Tick 驱动。
    /// 内部使用 Handle → Index 映射实现 O(1) 查找，移除时采用 swap-remove 模式。
    /// 不实现 ITickable，Tick 为独立方法，由 QuestManager 按需驱动。
    /// </summary>
    /// <typeparam name="TId">任务拥有者标识类型，必须可判等。</typeparam>
    public sealed class QuestSystem<TId> : IQuestSystem<TId> where TId : IEquatable<TId>
    {
        private readonly List<QuestEntry> _activeQuests = new();
        private readonly Dictionary<QuestHandle, int> _handleIndexMap = new();
        private readonly Func<int, QuestDef?> _getQuestDef;
        private int _nextHandleId = 1;

        /// <inheritdoc />
        public event Action<QuestStateChangedEvent<TId>>? OnQuestStateChanged;

        /// <inheritdoc />
        public event Action<QuestProgressUpdatedEvent>? OnQuestProgressUpdated;

        /// <summary>
        /// 初始化任务系统。
        /// </summary>
        /// <param name="getQuestDef">配置表查询委托，通过 questId 获取 <see cref="QuestDef"/>。不可为 null。</param>
        public QuestSystem(Func<int, QuestDef?> getQuestDef)
        {
            _getQuestDef = getQuestDef ?? throw new ArgumentNullException(nameof(getQuestDef));
        }

        /// <inheritdoc />
        public QuestHandle AcceptQuest(int questId, TId ownerId)
        {
            var questDef = _getQuestDef(questId);
            if (questDef == null)
            {
                return QuestHandle.Invalid;
            }

            var def = questDef.Value;

            // 检查 owner 是否已有同样 questId 的活跃任务（Active/Completed 状态不允许重复接取）
            if (TryFindQuestByOwnerAndId(ownerId, questId, out _))
            {
                return QuestHandle.Invalid;
            }

            // 通过引用池创建 QuestInstance
            var instance = ReferencePool.Acquire<QuestInstance>();
            instance.QuestId = questId;
            instance.State = QuestState.Active;
            instance.StartTime = Fixed64.Zero;
            instance.Progress = null;
            instance.RepeatCount = 0;
            instance.TimeRemaining = def.TimeLimit;

            var handle = new QuestHandle(_nextHandleId++);
            var entry = new QuestEntry
            {
                Handle = handle,
                Instance = instance,
                OwnerId = ownerId,
            };

            _activeQuests.Add(entry);
            _handleIndexMap[handle] = _activeQuests.Count - 1;

            var stateEvent = new QuestStateChangedEvent<TId>(handle, ownerId, QuestState.Locked, QuestState.Active, questId);
            OnQuestStateChanged?.Invoke(stateEvent);

            return handle;
        }

        /// <inheritdoc />
        public bool CompleteQuest(QuestHandle handle)
        {
            if (!TryGetEntry(handle, out var entry, out _))
            {
                return false;
            }

            var instance = entry.Instance;

            // Locked/Failed/Claimed 状态不允许完成
            if (instance.State != QuestState.Active)
            {
                return false;
            }

            var oldState = instance.State;
            instance.State = QuestState.Completed;

            var stateEvent = new QuestStateChangedEvent<TId>(handle, entry.OwnerId, oldState, QuestState.Completed, instance.QuestId);
            OnQuestStateChanged?.Invoke(stateEvent);

            return true;
        }

        /// <inheritdoc />
        public RewardDeliveryResult ClaimQuest(QuestHandle handle)
        {
            if (!TryGetEntry(handle, out var entry, out _))
            {
                return RewardDeliveryResult.FailureResult("Quest not found.");
            }

            var instance = entry.Instance;

            if (instance.State != QuestState.Completed)
            {
                return RewardDeliveryResult.FailureResult("Quest is not in Completed state.");
            }

            var oldState = instance.State;
            instance.State = QuestState.Claimed;

            var stateEvent = new QuestStateChangedEvent<TId>(handle, entry.OwnerId, oldState, QuestState.Claimed, instance.QuestId);
            OnQuestStateChanged?.Invoke(stateEvent);

            // 奖励实际由 QuestManager 通过 RewardProcessor 分发，此处返回空成功结果
            return RewardDeliveryResult.SuccessResult();
        }

        /// <inheritdoc />
        public bool AbandonQuest(QuestHandle handle)
        {
            if (!_handleIndexMap.TryGetValue(handle, out int index))
            {
                return false;
            }

            var entry = _activeQuests[index];
            var instance = entry.Instance;

            if (instance.State != QuestState.Active)
            {
                return false;
            }

            // Swap-remove 模式移除条目（从后往前替换）
            RemoveAt(index);

            // 归还引用池
            ReferencePool.Release(instance);

            return true;
        }

        /// <inheritdoc />
        public void Tick(Fixed64 deltaTime)
        {
            if (deltaTime <= Fixed64.Zero || _activeQuests.Count == 0)
            {
                return;
            }

            for (int i = _activeQuests.Count - 1; i >= 0; i--)
            {
                var entry = _activeQuests[i];
                var instance = entry.Instance;

                if (instance.State != QuestState.Active)
                {
                    continue;
                }

                if (instance.TimeRemaining == null)
                {
                    continue;
                }

                instance.TimeRemaining -= deltaTime;

                if (instance.TimeRemaining.Value <= Fixed64.Zero)
                {
                    var oldState = instance.State;
                    instance.State = QuestState.Failed;

                    var stateEvent = new QuestStateChangedEvent<TId>(
                        entry.Handle, entry.OwnerId, oldState, QuestState.Failed, instance.QuestId);
                    OnQuestStateChanged?.Invoke(stateEvent);
                }
            }
        }

        /// <inheritdoc />
        public QuestInstance? GetQuest(QuestHandle handle)
        {
            if (!_handleIndexMap.TryGetValue(handle, out int index))
            {
                return null;
            }

            return _activeQuests[index].Instance;
        }

        /// <inheritdoc />
        public IReadOnlyList<QuestInstance> GetAllQuests(TId ownerId)
        {
            var result = new List<QuestInstance>();
            var comparer = EqualityComparer<TId>.Default;

            for (int i = 0; i < _activeQuests.Count; i++)
            {
                var entry = _activeQuests[i];
                if (comparer.Equals(entry.OwnerId, ownerId))
                {
                    result.Add(entry.Instance);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _activeQuests.Count; i++)
                {
                    if (_activeQuests[i].Instance.State == QuestState.Active)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        // ==================== 内部实现 ====================

        /// <summary>
        /// 查找指定 owner 是否有相同 questId 的活跃/已完成任务。
        /// </summary>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <param name="questId">任务定义 ID。</param>
        /// <param name="index">找到的索引，未找到时为 -1。</param>
        /// <returns>找到返回 true，否则 false。</returns>
        private bool TryFindQuestByOwnerAndId(TId ownerId, int questId, out int index)
        {
            var comparer = EqualityComparer<TId>.Default;

            for (int i = 0; i < _activeQuests.Count; i++)
            {
                var entry = _activeQuests[i];
                if (entry.Instance.QuestId == questId &&
                    comparer.Equals(entry.OwnerId, ownerId) &&
                    (entry.Instance.State == QuestState.Active || entry.Instance.State == QuestState.Completed))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// 通过 Handle 获取对应条目及其索引。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <param name="entry">输出条目引用。</param>
        /// <param name="index">输出列表索引。</param>
        /// <returns>找到返回 true，否则 false。</returns>
        private bool TryGetEntry(QuestHandle handle, out QuestEntry entry, out int index)
        {
            if (!_handleIndexMap.TryGetValue(handle, out index))
            {
                entry = default!;
                return false;
            }

            entry = _activeQuests[index];
            return true;
        }

        /// <summary>
        /// Swap-remove 模式移除指定索引处的条目。
        /// 将末尾元素移到删除位置，更新索引映射，最后移除末尾。
        /// </summary>
        /// <param name="index">要移除的索引。</param>
        private void RemoveAt(int index)
        {
            var removedEntry = _activeQuests[index];
            var lastIndex = _activeQuests.Count - 1;

            if (index < lastIndex)
            {
                _activeQuests[index] = _activeQuests[lastIndex];
                _handleIndexMap[_activeQuests[index].Handle] = index;
            }

            _activeQuests.RemoveAt(lastIndex);
            _handleIndexMap.Remove(removedEntry.Handle);
        }

        /// <summary>
        /// 任务条目。封装 QuestInstance 与运行时元数据（Handle、OwnerId）。
        /// 与 <see cref="Combat.BuffSystem{TId}.BuffInstance"/> 模式一致，QuestInstance 自身仅存储
        /// QuestId 和序列化状态，Handle/OwnerId 作为运行时元数据由此条目管理。
        /// </summary>
        private sealed class QuestEntry
        {
            public QuestHandle Handle;
            public QuestInstance Instance = null!;
            public TId OwnerId = default!;
        }
    }
}
