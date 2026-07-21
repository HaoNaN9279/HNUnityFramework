#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common.Math;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 奖励处理器。
    /// 管理一组 <see cref="IRewardHandler{TId}"/>，按奖励组批量分发奖励给指定接收者。
    /// </summary>
    /// <typeparam name="TId">实体标识类型（必须实现 <see cref="IEquatable{T}"/>）。</typeparam>
    public sealed class RewardProcessor<TId> where TId : IEquatable<TId>
    {
        private readonly List<IRewardHandler<TId>> _handlers = new();
        private readonly HNRandom _random;

        /// <summary>
        /// 初始化 <see cref="RewardProcessor{TId}"/> 的新实例。
        /// </summary>
        /// <param name="handlers">可选的初始处理器集合。</param>
        /// <param name="random">可选的随机数生成器，为 null 时使用默认实例。</param>
        public RewardProcessor(IEnumerable<IRewardHandler<TId>>? handlers = null, HNRandom? random = null)
        {
            if (handlers != null)
            {
                _handlers.AddRange(handlers);
            }

            _random = random ?? new HNRandom();
        }

        /// <summary>注册一个奖励处理器。</summary>
        /// <param name="handler">要注册的处理器。</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出。</exception>
        public void RegisterHandler(IRewardHandler<TId> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.Add(handler);
        }

        /// <summary>移除一个奖励处理器。</summary>
        /// <param name="handler">要移除的处理器。若不存在则无操作。</param>
        public void UnregisterHandler(IRewardHandler<TId> handler)
        {
            _handlers.Remove(handler);
        }

        /// <summary>
        /// 处理指定奖励组，向接收者发放奖励。
        /// 遍历每个奖励组获取奖励定义，进行概率判定后分发给匹配的处理器。
        /// </summary>
        /// <param name="rewardGroupIds">奖励组 ID 列表。</param>
        /// <param name="getRewardsForGroup">根据奖励组 ID 获取奖励定义列表的委托。</param>
        /// <param name="recipient">奖励接收者标识。</param>
        /// <returns>发放结果，包含成功/失败状态和已发放条目。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="getRewardsForGroup"/> 为 null 时抛出。</exception>
        public RewardDeliveryResult ProcessRewards(
            IReadOnlyList<int> rewardGroupIds,
            Func<int, IReadOnlyList<RewardDef>?> getRewardsForGroup,
            TId recipient)
        {
            if (rewardGroupIds == null || rewardGroupIds.Count == 0)
            {
                return RewardDeliveryResult.SuccessResult();
            }

            if (getRewardsForGroup == null)
            {
                throw new ArgumentNullException(nameof(getRewardsForGroup));
            }

            var allDelivered = new List<(RewardType, int, int)>();

            foreach (var groupId in rewardGroupIds)
            {
                var rewards = getRewardsForGroup(groupId);
                if (rewards == null)
                {
                    continue;
                }

                foreach (var reward in rewards)
                {
                    // 概率判定
                    if (reward.Probability < 1.0f)
                    {
                        var roll = _random.NextFloat();
                        if (roll > reward.Probability)
                        {
                            continue;
                        }
                    }

                    // 查找合适的 Handler
                    IRewardHandler<TId>? handler = null;
                    foreach (var h in _handlers)
                    {
                        if (h.CanHandle(reward))
                        {
                            handler = h;
                            break;
                        }
                    }

                    if (handler != null)
                    {
                        var result = handler.Deliver(reward, recipient);
                        if (result.Success && result.DeliveredItems != null)
                        {
                            allDelivered.AddRange(result.DeliveredItems);
                        }
                    }
                }
            }

            return RewardDeliveryResult.SuccessResult(allDelivered);
        }

        /// <summary>清空所有已注册的处理器。</summary>
        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
