#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 成就系统接口。管理成就的完整生命周期。
    /// </summary>
    /// <typeparam name="TId">拥有者标识类型。</typeparam>
    public interface IAchievementSystem<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// 揭示隐藏成就。
        /// </summary>
        /// <param name="achievementId">成就定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>揭示成功返回 true，否则 false。</returns>
        bool RevealAchievement(int achievementId, TId ownerId);

        /// <summary>
        /// 完成成就。
        /// </summary>
        /// <param name="achievementId">成就定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>完成成功返回 true，否则 false。</returns>
        bool CompleteAchievement(int achievementId, TId ownerId);

        /// <summary>
        /// 领取成就奖励。
        /// </summary>
        /// <param name="achievementId">成就定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>奖励发放结果。</returns>
        RewardDeliveryResult ClaimAchievement(int achievementId, TId ownerId);

        /// <summary>
        /// 获取指定成就实例。
        /// </summary>
        /// <param name="achievementId">成就定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>成就实例，不存在时返回 null。</returns>
        AchievementInstance? GetAchievement(int achievementId, TId ownerId);

        /// <summary>
        /// 获取所有成就实例。
        /// </summary>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>成就实例只读列表。</returns>
        IReadOnlyList<AchievementInstance> GetAllAchievements(TId ownerId);

        /// <summary>
        /// 获取指定分类下的成就实例。
        /// </summary>
        /// <param name="categoryId">分类 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>成就实例只读列表。</returns>
        IReadOnlyList<AchievementInstance> GetByCategory(int categoryId, TId ownerId);

        /// <summary>已完成的成就数量。</summary>
        int CompletedCount { get; }

        /// <summary>成就状态变更事件。</summary>
        event Action<AchievementStateChangedEvent<TId>>? OnAchievementStateChanged;

        /// <summary>成就进度更新事件。</summary>
        event Action<AchievementProgressUpdatedEvent>? OnAchievementProgressUpdated;
    }
}
