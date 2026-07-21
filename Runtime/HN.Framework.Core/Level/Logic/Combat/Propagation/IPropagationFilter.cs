using System;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 传播过滤器接口。定义目标筛选条件。
    /// 用于在 <see cref="IPropagationStrategy{TId}"/> 找到候选目标后进一步过滤。
    /// </summary>
    /// <typeparam name="TId">实体标识类型。</typeparam>
    public interface IPropagationFilter<TId> where TId : IEquatable<TId>
    {
        /// <summary>判断目标是否通过过滤器。</summary>
        /// <param name="target">候选目标标识。</param>
        /// <param name="context">当前传播上下文。</param>
        /// <returns>如果目标应被命中则返回 true。</returns>
        bool Pass(TId target, PropagationContext<TId> context);
    }
}
