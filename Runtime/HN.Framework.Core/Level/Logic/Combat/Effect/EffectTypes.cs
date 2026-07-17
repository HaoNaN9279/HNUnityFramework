#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 效果类型枚举。
    /// </summary>
    public enum EffectType : byte
    {
        /// <summary>瞬时效果：应用后立即生效并移除。</summary>
        Instant = 0,
        /// <summary>持续效果：在持续时间内生效，到期自动移除。</summary>
        Duration = 1,
        /// <summary>无限效果：永久有效，直到被手动移除。</summary>
        Infinite = 2,
    }

    /// <summary>
    /// 属性修正器对：指定目标属性类型 + 要应用的 Modifier。
    /// Effect 通过此结构描述对属性的具体修正操作。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public readonly struct AttributeModifier<TId> where TId : IEquatable<TId>
    {
        /// <summary>目标属性类型。</summary>
        public readonly AttributeType TargetAttribute;

        /// <summary>要应用的 Modifier。</summary>
        public readonly Modifier<TId> Modifier;

        public AttributeModifier(AttributeType targetAttribute, Modifier<TId> modifier)
        {
            TargetAttribute = targetAttribute;
            Modifier = modifier;
        }
    }

    /// <summary>
    /// 效果规格定义。描述一个效果的完整数据，通常来自配置表。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class EffectSpec<TId> where TId : IEquatable<TId>
    {
        /// <summary>效果定义 ID。</summary>
        public int EffectId { get; set; }

        /// <summary>效果类型。</summary>
        public EffectType Type { get; set; } = EffectType.Instant;

        /// <summary>持续时间（秒），仅 <see cref="EffectType.Duration"/> 有效。</summary>
        public Fixed64 Duration { get; set; }

        /// <summary>周期性触发间隔（秒），仅 Duration 类型有效。0 表示不触发周期效果。</summary>
        public Fixed64 PeriodInterval { get; set; }

        /// <summary>属性修正器列表：指定对哪些属性的哪些 Modifier。</summary>
        public List<AttributeModifier<TId>> AttributeModifiers { get; set; } = new();

        /// <summary>是否延迟生效。</summary>
        public bool HasDelay { get; set; }

        /// <summary>延迟时间（秒）。</summary>
        public Fixed64 DelayTime { get; set; }

        /// <summary>效果名称（调试用）。</summary>
        public string? DisplayName { get; set; }
    }

    /// <summary>
    /// 效果句柄。用于追踪和移除正在运行的效果。
    /// </summary>
    public readonly struct EffectHandle : IEquatable<EffectHandle>
    {
        internal readonly int Id;
        public bool IsValid => Id > 0;
        internal EffectHandle(int id) { Id = id; }
        public static readonly EffectHandle Invalid = default;

        public bool Equals(EffectHandle other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is EffectHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(EffectHandle left, EffectHandle right) => left.Equals(right);
        public static bool operator !=(EffectHandle left, EffectHandle right) => !left.Equals(right);
    }
}
