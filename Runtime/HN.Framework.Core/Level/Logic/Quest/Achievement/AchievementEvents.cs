#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 成就状态变更事件。
    /// </summary>
    /// <typeparam name="TId">拥有者标识类型。</typeparam>
    public readonly struct AchievementStateChangedEvent<TId> where TId : IEquatable<TId>
    {
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;

        /// <summary>成就 ID。</summary>
        public readonly int AchievementId;

        /// <summary>旧状态。</summary>
        public readonly AchievementState OldState;

        /// <summary>新状态。</summary>
        public readonly AchievementState NewState;

        /// <summary>
        /// 初始化状态变更事件。
        /// </summary>
        public AchievementStateChangedEvent(TId ownerId, int achievementId, AchievementState oldState, AchievementState newState)
        {
            OwnerId = ownerId;
            AchievementId = achievementId;
            OldState = oldState;
            NewState = newState;
        }
    }

    /// <summary>
    /// 成就揭示事件。
    /// </summary>
    /// <typeparam name="TId">拥有者标识类型。</typeparam>
    public readonly struct AchievementRevealedEvent<TId> where TId : IEquatable<TId>
    {
        public readonly TId OwnerId;
        public readonly int AchievementId;

        public AchievementRevealedEvent(TId ownerId, int achievementId)
        {
            OwnerId = ownerId;
            AchievementId = achievementId;
        }
    }

    /// <summary>
    /// 成就进度更新事件。
    /// </summary>
    public readonly struct AchievementProgressUpdatedEvent
    {
        /// <summary>成就 ID。</summary>
        public readonly int AchievementId;

        /// <summary>更新前进度。</summary>
        public readonly float PrevProgress;

        /// <summary>更新后进度。</summary>
        public readonly float NewProgress;

        public AchievementProgressUpdatedEvent(int achievementId, float prevProgress, float newProgress)
        {
            AchievementId = achievementId;
            PrevProgress = prevProgress;
            NewProgress = newProgress;
        }
    }

    /// <summary>
    /// 成就领取事件。
    /// </summary>
    /// <typeparam name="TId">拥有者标识类型。</typeparam>
    public readonly struct AchievementClaimedEvent<TId> where TId : IEquatable<TId>
    {
        public readonly TId OwnerId;
        public readonly int AchievementId;

        public AchievementClaimedEvent(TId ownerId, int achievementId)
        {
            OwnerId = ownerId;
            AchievementId = achievementId;
        }
    }
}
