#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 传播规格定义。描述一次传播行为的配置数据，通常来自技能/效果配置表。
    /// </summary>
    /// <typeparam name="TId">实体标识类型。</typeparam>
    public sealed class PropagationSpec<TId> where TId : IEquatable<TId>
    {
        /// <summary>传播定义 ID。</summary>
        public int PropagationId { get; set; }

        /// <summary>最大传播跳数。1 表示仅命中初始目标，&gt;1 表示可传播到次级目标。</summary>
        public int MaxHops { get; set; } = 1;

        /// <summary>每跳后的强度衰减系数（乘法）。例如 0.5 表示每跳效果减半。范围 (0, 1]。</summary>
        public Fixed64 IntensityDecay { get; set; } = Fixed64.One;

        /// <summary>命中目标时施加的效果列表。</summary>
        public List<EffectSpec<TId>> OnHitEffects { get; set; } = new();
    }

    /// <summary>
    /// 传播上下文。在传播过程中承载运行态数据，在各跳之间共享。
    /// </summary>
    /// <typeparam name="TId">实体标识类型。</typeparam>
    public sealed class PropagationContext<TId> where TId : IEquatable<TId>
    {
        /// <summary>传播规格引用。</summary>
        public readonly PropagationSpec<TId> Spec;

        /// <summary>当前跳数（0-based）。0 表示第一跳。</summary>
        public int CurrentHop;

        /// <summary>当前强度倍率（初始为 1.0，每跳乘以 <see cref="PropagationSpec{TId}.IntensityDecay"/>）。</summary>
        public Fixed64 IntensityMultiplier = Fixed64.One;

        /// <summary>已命中实体集合（用于去重，防止循环传播）。</summary>
        public readonly HashSet<TId> HitEntities = new();

        /// <summary>传播是否已完成（外部可设置此标志提前终止）。</summary>
        public bool IsComplete;

        /// <summary>传播源标识。</summary>
        public readonly TId Source;

        public PropagationContext(PropagationSpec<TId> spec, TId source)
        {
            Spec = spec;
            Source = source;
        }
    }

    /// <summary>
    /// 传播句柄。用于追踪和标识一次传播执行实例。
    /// 结构遵循 <see cref="ModifierHandle"/> / <see cref="EffectHandle"/> / <see cref="BuffHandle"/> 统一模式。
    /// </summary>
    public readonly struct PropagationHandle : IEquatable<PropagationHandle>
    {
        internal readonly int Id;

        /// <summary>是否有效（Id > 0）。</summary>
        public bool IsValid => Id > 0;

        internal PropagationHandle(int id) { Id = id; }

        /// <summary>无效句柄。</summary>
        public static readonly PropagationHandle Invalid = default;

        public bool Equals(PropagationHandle other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is PropagationHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(PropagationHandle left, PropagationHandle right) => left.Equals(right);
        public static bool operator !=(PropagationHandle left, PropagationHandle right) => !left.Equals(right);
    }

    /// <summary>
    /// 传播结果。包含一次传播执行的完整统计信息。
    /// </summary>
    /// <typeparam name="TId">实体标识类型。</typeparam>
    public sealed class PropagationResult<TId> where TId : IEquatable<TId>
    {
        /// <summary>传播句柄。</summary>
        public PropagationHandle Handle { get; }

        /// <summary>命中的实体列表（按命中顺序，不含重复）。</summary>
        public List<TId> HitEntities { get; }

        /// <summary>实际传播跳数。</summary>
        public int TotalHops { get; }

        /// <summary>是否执行成功（至少命中一个目标）。</summary>
        public bool Success { get; }

        /// <summary>空结果单例。</summary>
        public static readonly PropagationResult<TId> Empty = new(
            PropagationHandle.Invalid,
            new List<TId>(),
            0,
            false);

        public PropagationResult(PropagationHandle handle, List<TId> hitEntities, int totalHops, bool success)
        {
            Handle = handle;
            HitEntities = hitEntities;
            TotalHops = totalHops;
            Success = success;
        }
    }
}
