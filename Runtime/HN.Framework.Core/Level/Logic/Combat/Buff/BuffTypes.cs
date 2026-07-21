#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// Buff 层叠规则。
    /// </summary>
    public enum BuffStackRule : byte
    {
        /// <summary>单一：不可堆叠，新 Buff 覆盖旧 Buff。</summary>
        Single = 0,

        /// <summary>多层：可堆叠至 <see cref="BuffSpec.MaxStacks"/>。</summary>
        Multi = 1,

        /// <summary>刷新：仅刷新持续时间，不增加层数。</summary>
        Refresh = 2,

        /// <summary>延长：增加持续时间，不增加层数。</summary>
        Extend = 3,
    }

    /// <summary>
    /// Buff 规格定义。描述一个 Buff 的完整数据，通常来自配置表。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class BuffSpec<TId> where TId : IEquatable<TId>
    {
        /// <summary>Buff 定义 ID。</summary>
        public int BuffId { get; set; }

        /// <summary>关联的 Behaviour 定义 ID。0 表示无自定义行为。</summary>
        public int BehaviourId { get; set; }

        /// <summary>显示名称。</summary>
        public string? DisplayName { get; set; }

        /// <summary>持续时间（秒）。</summary>
        public Fixed64 Duration { get; set; }

        /// <summary>层叠规则。</summary>
        public BuffStackRule StackRule { get; set; } = BuffStackRule.Single;

        /// <summary>最大层数（仅 <see cref="BuffStackRule.Multi"/> 有效）。</summary>
        public int MaxStacks { get; set; } = 1;

        /// <summary>每层额外持续时间（仅 <see cref="BuffStackRule.Extend"/> 有效）。</summary>
        public Fixed64 ExtraDurationPerStack { get; set; }

        /// <summary>应用时触发效果的列表。</summary>
        public List<EffectSpec<TId>> ApplyEffects { get; set; } = new();

        /// <summary>移除时触发效果的列表。</summary>
        public List<EffectSpec<TId>> RemoveEffects { get; set; } = new();

        /// <summary>周期性触发效果列表。</summary>
        public List<PeriodicEffectSpec<TId>> PeriodicEffects { get; set; } = new();

        /// <summary>可选标签索引（如 GameplayTag）。</summary>
        public HashSet<int>? TagIndices { get; set; }

        /// <summary>传递给 Behaviour 的自定义参数。由 Behaviour 实现方定义键值约定。</summary>
        public Dictionary<string, Fixed64>? BehaviourCustomParams { get; set; }
    }

    /// <summary>
    /// 周期性效果规格。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class PeriodicEffectSpec<TId> where TId : IEquatable<TId>
    {
        /// <summary>触发间隔（秒）。</summary>
        public Fixed64 Interval { get; set; }

        /// <summary>触发的效果。</summary>
        public EffectSpec<TId> Effect { get; set; } = new();

        /// <summary>延迟首次触发的时间（秒）。</summary>
        public Fixed64 InitialDelay { get; set; }
    }

    /// <summary>
    /// Buff 句柄。用于追踪和移除运行中的 Buff。
    /// </summary>
    public readonly struct BuffHandle : IEquatable<BuffHandle>
    {
        internal readonly int Id;
        public bool IsValid => Id > 0;
        internal BuffHandle(int id) { Id = id; }
        public static readonly BuffHandle Invalid = default;

        public bool Equals(BuffHandle other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is BuffHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(BuffHandle left, BuffHandle right) => left.Equals(right);
        public static bool operator !=(BuffHandle left, BuffHandle right) => !left.Equals(right);
    }
}
