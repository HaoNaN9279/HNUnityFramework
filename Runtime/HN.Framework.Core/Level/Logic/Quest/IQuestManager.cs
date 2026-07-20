#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// QuestManager 统一接口。整合 QuestSystem、AchievementSystem、ConditionEvaluator、
    /// RewardProcessor 和 QuestChain，提供统一的任务/成就生命周期管理入口。
    /// </summary>
    /// <typeparam name="TId">任务/成就拥有者标识类型，必须可判等。</typeparam>
    public interface IQuestManager<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// 初始化 QuestManager，加载所有配置表数据。
        /// 必须在任何其他操作之前调用。
        /// </summary>
        /// <param name="questDefs">任务定义列表。</param>
        /// <param name="achievementDefs">成就定义列表。</param>
        /// <param name="conditionGroups">条件组定义列表。</param>
        /// <param name="rewards">奖励定义列表。</param>
        /// <param name="questChains">任务链定义列表。</param>
        void Initialize(
            IEnumerable<QuestDef> questDefs,
            IEnumerable<AchievementDef> achievementDefs,
            IEnumerable<ConditionGroupDef> conditionGroups,
            IEnumerable<RewardDef> rewards,
            IEnumerable<QuestChainDef> questChains);

        // ==================== 任务操作 ====================

        /// <summary>
        /// 尝试接受任务。验证前置条件、等级、任务链约束及重复接取后，正式接受任务。
        /// </summary>
        /// <param name="questId">任务定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>成功返回有效句柄，失败返回 <see cref="QuestHandle.Invalid"/>。</returns>
        QuestHandle TryAcceptQuest(int questId, TId ownerId);

        /// <summary>
        /// 完成任务（条件全部满足时调用）。将任务状态从 Active 转为 Completed。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>完成成功返回 true，否则 false。</returns>
        bool CompleteQuest(QuestHandle handle);

        /// <summary>
        /// 领取任务奖励。发放奖励后将任务标记为 Claimed。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>奖励发放结果。</returns>
        RewardDeliveryResult ClaimQuest(QuestHandle handle);

        /// <summary>
        /// 使任务失败。将任务状态从 Active 转为 Failed。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>失败操作是否成功。</returns>
        bool FailQuest(QuestHandle handle);

        /// <summary>
        /// 更新任务进度。按 progressKey 增加 delta 值。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <param name="progressKey">进度键名。</param>
        /// <param name="delta">增量值。</param>
        void UpdateQuestProgress(QuestHandle handle, string progressKey, int delta);

        // ==================== 成就操作 ====================

        /// <summary>
        /// 完成成就。隐藏成就自动 Reveal 后再 Complete。
        /// </summary>
        /// <param name="achievementId">成就定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>完成成功返回 true，否则 false。</returns>
        bool CompleteAchievement(int achievementId, TId ownerId);

        /// <summary>
        /// 领取成就奖励。发放奖励后将成就标记为 Claimed。
        /// </summary>
        /// <param name="achievementId">成就定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>奖励发放结果。</returns>
        RewardDeliveryResult ClaimAchievement(int achievementId, TId ownerId);

        // ==================== 条件事件注册 ====================

        /// <summary>
        /// 注册 Counter 条件需要监听的游戏事件类型。
        /// 项目方通过此 API 将游戏事件绑定到条件系统。
        /// </summary>
        /// <typeparam name="T">事件数据类型。</typeparam>
        /// <param name="eventTypeName">事件类型名称，对应 ConditionDef.Parameters["EventTypeName"]。</param>
        void RegisterCounterEventType<T>(string eventTypeName);

        /// <summary>
        /// 手动通知游戏事件发生（不使用 EventBus 时）。
        /// 直接转发到 ConditionEvaluator 驱动 Counter 条件计数。
        /// </summary>
        /// <param name="eventTypeName">事件类型名称。</param>
        /// <param name="ownerId">事件关联的实体 ID（可选）。</param>
        void NotifyGameEvent(string eventTypeName, TId? ownerId = default);

        // ==================== 查询 ====================

        /// <summary>
        /// 通过句柄获取任务实例。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>任务实例，无效句柄时返回 null。</returns>
        QuestInstance? GetQuest(QuestHandle handle);

        /// <summary>
        /// 获取指定拥有者的成就实例。
        /// </summary>
        /// <param name="achievementId">成就定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>成就实例，不存在时返回 null。</returns>
        AchievementInstance? GetAchievement(int achievementId, TId ownerId);

        /// <summary>
        /// 获取指定拥有者的任务链实例。
        /// </summary>
        /// <param name="chainId">任务链定义 ID。</param>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>任务链实例，不存在时返回 null。</returns>
        QuestChainInstance? GetQuestChain(int chainId, TId ownerId);

        /// <summary>
        /// 当前活跃（Active 状态）任务数量。
        /// </summary>
        int ActiveQuestCount { get; }

        /// <summary>
        /// 已完成（Claimed 状态）成就数量。
        /// </summary>
        int CompletedAchievementCount { get; }
    }
}
