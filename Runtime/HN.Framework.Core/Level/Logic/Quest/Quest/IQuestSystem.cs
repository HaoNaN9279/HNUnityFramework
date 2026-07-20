#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务系统接口。管理任务的生命周期，包括接受、完成、领取奖励、放弃和 Tick 驱动。
    /// 不继承 ITickable，通过独立的 Tick 方法驱动。
    /// </summary>
    /// <typeparam name="TId">任务拥有者标识类型，必须可判等。</typeparam>
    public interface IQuestSystem<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// 接受任务。
        /// </summary>
        /// <param name="questId">任务定义 ID。</param>
        /// <param name="ownerId">任务拥有者 ID。</param>
        /// <returns>分配的任务句柄。</returns>
        QuestHandle AcceptQuest(int questId, TId ownerId);

        /// <summary>
        /// 完成任务（条件全部满足时）。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>完成成功返回 true，否则 false。</returns>
        bool CompleteQuest(QuestHandle handle);

        /// <summary>
        /// 领取任务奖励。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>奖励发放结果。</returns>
        RewardDeliveryResult ClaimQuest(QuestHandle handle);

        /// <summary>
        /// 放弃任务。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>放弃成功返回 true，否则 false。</returns>
        bool AbandonQuest(QuestHandle handle);

        /// <summary>
        /// 任务系统每帧驱动。处理超时检查等逻辑。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间。</param>
        void Tick(Fixed64 deltaTime);

        /// <summary>
        /// 通过句柄获取任务实例。
        /// </summary>
        /// <param name="handle">任务句柄。</param>
        /// <returns>任务实例，无效句柄时返回 null。</returns>
        QuestInstance? GetQuest(QuestHandle handle);

        /// <summary>
        /// 获取指定拥有者的所有任务。
        /// </summary>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <returns>只读任务实例列表。</returns>
        IReadOnlyList<QuestInstance> GetAllQuests(TId ownerId);

        /// <summary>
        /// 当前进行中的任务数量。
        /// </summary>
        int ActiveCount { get; }

        /// <summary>
        /// 任务状态变更事件。
        /// </summary>
        event Action<QuestStateChangedEvent<TId>>? OnQuestStateChanged;

        /// <summary>
        /// 任务进度更新事件。
        /// </summary>
        event Action<QuestProgressUpdatedEvent>? OnQuestProgressUpdated;
    }
}
