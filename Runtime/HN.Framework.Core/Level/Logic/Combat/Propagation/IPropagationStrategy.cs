using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 传播策略接口。定义如何寻找传播目标。
    /// 具体实现（半径扫描、链式弹跳等）由项目层提供。
    /// </summary>
    /// <typeparam name="TId">实体标识类型。</typeparam>
    public interface IPropagationStrategy<TId> where TId : IEquatable<TId>
    {
        /// <summary>根据传播上下文查找下一跳的目标列表。</summary>
        /// <param name="context">当前传播上下文，包含跳数、强度、已命中实体等状态。</param>
        /// <param name="results">输出参数，找到的目标将添加到此列表。</param>
        void FindTargets(PropagationContext<TId> context, List<TId> results);
    }
}
