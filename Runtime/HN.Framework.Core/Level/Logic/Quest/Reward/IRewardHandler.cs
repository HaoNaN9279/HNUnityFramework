#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 奖励处理器接口。
    /// 项目方需要为每种 <see cref="RewardType"/> 实现具体的发放逻辑。
    /// </summary>
    /// <typeparam name="TId">实体标识类型（必须实现 <see cref="IEquatable{T}"/>）。</typeparam>
    public interface IRewardHandler<TId> where TId : IEquatable<TId>
    {
        /// <summary>该处理器能处理的奖励类型。</summary>
        RewardType HandledType { get; }

        /// <summary>判断是否能处理指定的奖励定义。</summary>
        /// <param name="reward">奖励配置定义。</param>
        /// <returns>true 表示可以处理该奖励。</returns>
        bool CanHandle(RewardDef reward);

        /// <summary>向指定接收者发放奖励。</summary>
        /// <param name="reward">奖励配置定义。</param>
        /// <param name="recipient">接收者标识。</param>
        /// <returns>发放结果。</returns>
        RewardDeliveryResult Deliver(RewardDef reward, TId recipient);
    }
}
