#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// QuestManager 初始化完成事件。
    /// 由 QuestManager 在所有子系统初始化完毕后通过 EventBus 发布。
    /// </summary>
    public readonly struct QuestManagerInitializedEvent
    {
    }

    /// <summary>
    /// 奖励领取事件。
    /// 由 RewardManager 在奖励发放完成后通过 EventBus 发布。
    /// </summary>
    /// <typeparam name="TId">领奖者 ID 类型。</typeparam>
    public readonly struct RewardClaimedEvent<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>领奖者 ID。</summary>
        public readonly TId RecipientId;

        /// <summary>关联的任务/成就定义 ID。</summary>
        public readonly int QuestId;

        /// <summary>奖励组 ID。</summary>
        public readonly int RewardGroupId;

        /// <summary>发放结果。</summary>
        public readonly RewardDeliveryResult Result;

        /// <summary>
        /// 初始化 RewardClaimedEvent 实例。
        /// </summary>
        /// <param name="recipientId">领奖者 ID。</param>
        /// <param name="questId">关联的任务/成就定义 ID。</param>
        /// <param name="rewardGroupId">奖励组 ID。</param>
        /// <param name="result">发放结果。</param>
        public RewardClaimedEvent(TId recipientId, int questId, int rewardGroupId, RewardDeliveryResult result)
        {
            RecipientId = recipientId;
            QuestId = questId;
            RewardGroupId = rewardGroupId;
            Result = result;
        }
    }

    /// <summary>
    /// 所有任务完成事件。
    /// 由 QuestManager 在某个 Owner 的全部任务完成后通过 EventBus 发布。
    /// </summary>
    /// <typeparam name="TId">拥有者 ID 类型。</typeparam>
    public readonly struct AllQuestsCompletedEvent<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;

        /// <summary>
        /// 初始化 AllQuestsCompletedEvent 实例。
        /// </summary>
        /// <param name="ownerId">拥有者 ID。</param>
        public AllQuestsCompletedEvent(TId ownerId)
        {
            OwnerId = ownerId;
        }
    }

    /// <summary>
    /// 任务链状态变更事件。
    /// 由 QuestChainSystem 在任务链状态发生变化时通过 EventBus 发布。
    /// </summary>
    /// <typeparam name="TId">拥有者 ID 类型。</typeparam>
    public readonly struct QuestChainStateChangedEvent<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>拥有者 ID。</summary>
        public readonly TId OwnerId;

        /// <summary>任务链定义 ID。</summary>
        public readonly int ChainId;

        /// <summary>变更前状态。</summary>
        public readonly QuestChainState OldState;

        /// <summary>变更后状态。</summary>
        public readonly QuestChainState NewState;

        /// <summary>
        /// 初始化 QuestChainStateChangedEvent 实例。
        /// </summary>
        /// <param name="ownerId">拥有者 ID。</param>
        /// <param name="chainId">任务链定义 ID。</param>
        /// <param name="oldState">变更前状态。</param>
        /// <param name="newState">变更后状态。</param>
        public QuestChainStateChangedEvent(TId ownerId, int chainId, QuestChainState oldState, QuestChainState newState)
        {
            OwnerId = ownerId;
            ChainId = chainId;
            OldState = oldState;
            NewState = newState;
        }
    }
}
