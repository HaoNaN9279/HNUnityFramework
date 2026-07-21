#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 成就系统实现。管理成就的生命周期：揭示、完成、领取。
    /// 使用 List + Dictionary Handle 模式追踪运行时实例。
    /// 成就是一次性的，不支持重复；无 Failed 状态。
    /// 不实现 ITickable。
    /// </summary>
    /// <typeparam name="TId">拥有者标识类型，必须可判等。</typeparam>
    public sealed class AchievementSystem<TId> : IAchievementSystem<TId> where TId : IEquatable<TId>
    {
        private readonly List<AchievementInstance> _activeAchievements = new();
        private readonly Dictionary<AchievementHandle, int> _handleIndexMap = new();
        private readonly Func<int, AchievementDef?> _getAchievementDef;
        private int _nextHandleId = 1;

        /// <summary>
        /// 拥有者反向索引：(achievementId, ownerId) → 在 _activeAchievements 中的索引。
        /// </summary>
        private readonly Dictionary<(int AchievementId, TId OwnerId), int> _achievementIndexMap = new();

        /// <inheritdoc />
        public event Action<AchievementStateChangedEvent<TId>>? OnAchievementStateChanged;

        /// <inheritdoc />
        public event Action<AchievementProgressUpdatedEvent>? OnAchievementProgressUpdated;

        /// <summary>
        /// 初始化成就系统。
        /// </summary>
        /// <param name="getAchievementDef">通过成就 ID 获取配置定义的委托。</param>
        /// <exception cref="ArgumentNullException"><paramref name="getAchievementDef"/> 为 null 时抛出。</exception>
        public AchievementSystem(Func<int, AchievementDef?> getAchievementDef)
        {
            _getAchievementDef = getAchievementDef ?? throw new ArgumentNullException(nameof(getAchievementDef));
        }

        /// <inheritdoc />
        public int CompletedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _activeAchievements.Count; i++)
                {
                    if (_activeAchievements[i].State == AchievementState.Claimed)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <inheritdoc />
        public bool RevealAchievement(int achievementId, TId ownerId)
        {
            var def = _getAchievementDef(achievementId);
            if (def == null)
            {
                return false;
            }

            var key = (achievementId, ownerId);
            if (_achievementIndexMap.ContainsKey(key))
            {
                return false;
            }

            // 检查前置成就：如果有前置成就列表，必须全部处于 Claimed 状态
            if (def.Value.PrerequisiteAchievementIds != null)
            {
                for (int i = 0; i < def.Value.PrerequisiteAchievementIds.Count; i++)
                {
                    int prereqId = def.Value.PrerequisiteAchievementIds[i];
                    if (!TryGetIndex(prereqId, ownerId, out int prereqIndex))
                    {
                        return false;
                    }

                    if (_activeAchievements[prereqIndex].State != AchievementState.Claimed)
                    {
                        return false;
                    }
                }
            }

            // 通过引用池创建实例
            var instance = ReferencePool.Acquire<AchievementInstance>();
            instance.Initialize(def.Value);

            var handle = new AchievementHandle(_nextHandleId++);

            int index = _activeAchievements.Count;
            _activeAchievements.Add(instance);
            _handleIndexMap[handle] = index;
            _achievementIndexMap[key] = index;

            // Initialize 已根据 IsHidden 设置初始状态：
            //   IsHidden=true  → Hidden  → RevealAchievement 负责过渡到 Revealed
            //   IsHidden=false → Revealed（直接 Active，无需额外过渡）
            // 统一触发状态变更事件：系统视角下从"未追踪"（Hidden）到 Revealed
            OnAchievementStateChanged?.Invoke(
                new AchievementStateChangedEvent<TId>(ownerId, achievementId, AchievementState.Hidden, AchievementState.Revealed));

            return true;
        }

        /// <inheritdoc />
        public bool CompleteAchievement(int achievementId, TId ownerId)
        {
            if (!TryGetIndex(achievementId, ownerId, out int index))
            {
                return false;
            }

            var instance = _activeAchievements[index];
            if (instance.State != AchievementState.Revealed)
            {
                return false;
            }

            var oldState = instance.State;
            var prevProgress = instance.ProgressPercent;

            instance.State = AchievementState.Completed;
            instance.ProgressPercent = 1.0f;
            instance.CompletedTime = Fixed64.Zero;

            OnAchievementStateChanged?.Invoke(
                new AchievementStateChangedEvent<TId>(ownerId, achievementId, oldState, AchievementState.Completed));
            OnAchievementProgressUpdated?.Invoke(
                new AchievementProgressUpdatedEvent(achievementId, prevProgress, 1.0f));

            return true;
        }

        /// <inheritdoc />
        public RewardDeliveryResult ClaimAchievement(int achievementId, TId ownerId)
        {
            if (!TryGetIndex(achievementId, ownerId, out int index))
            {
                return RewardDeliveryResult.FailureResult("Achievement not found.");
            }

            var instance = _activeAchievements[index];
            if (instance.State != AchievementState.Completed)
            {
                return RewardDeliveryResult.FailureResult("Achievement is not in Completed state.");
            }

            var oldState = instance.State;
            instance.State = AchievementState.Claimed;
            instance.ClaimedTime = Fixed64.Zero;

            OnAchievementStateChanged?.Invoke(
                new AchievementStateChangedEvent<TId>(ownerId, achievementId, oldState, AchievementState.Claimed));

            // 返回空的成功结果，实际奖励由 QuestManager 负责分发
            return RewardDeliveryResult.SuccessResult();
        }

        /// <inheritdoc />
        public AchievementInstance? GetAchievement(int achievementId, TId ownerId)
        {
            if (TryGetIndex(achievementId, ownerId, out int index))
            {
                return _activeAchievements[index];
            }

            return null;
        }

        /// <inheritdoc />
        public IReadOnlyList<AchievementInstance> GetAllAchievements(TId ownerId)
        {
            var result = new List<AchievementInstance>();
            foreach (var kvp in _achievementIndexMap)
            {
                var (achId, owner) = kvp.Key;
                if (EqualityComparer<TId>.Default.Equals(owner, ownerId))
                {
                    result.Add(_activeAchievements[kvp.Value]);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IReadOnlyList<AchievementInstance> GetByCategory(int categoryId, TId ownerId)
        {
            var result = new List<AchievementInstance>();
            foreach (var kvp in _achievementIndexMap)
            {
                var (achId, owner) = kvp.Key;
                if (!EqualityComparer<TId>.Default.Equals(owner, ownerId))
                {
                    continue;
                }

                var def = _getAchievementDef(achId);
                if (def != null && def.Value.CategoryId == categoryId)
                {
                    result.Add(_activeAchievements[kvp.Value]);
                }
            }

            return result;
        }

        /// <summary>
        /// 通过 (achievementId, ownerId) 查找在 _activeAchievements 中的索引。
        /// </summary>
        private bool TryGetIndex(int achievementId, TId ownerId, out int index)
        {
            return _achievementIndexMap.TryGetValue((achievementId, ownerId), out index);
        }
    }
}
