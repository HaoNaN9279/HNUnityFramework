#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// Modifier 运算类型。
    /// 计算顺序：<see cref="Override"/> → <see cref="Add"/>（按 Priority） → <see cref="Multiply"/>（按 Priority） → <see cref="MinCap"/> → <see cref="MaxCap"/>。
    /// </summary>
    public enum ModifierOp : byte
    {
        /// <summary>覆盖为指定值。此类型的 Modifier 按 Priority 排序，最高优先级的生效。</summary>
        Override = 0,

        /// <summary>加法：FinalValue += Value。按 Priority 从小到大依次累加。</summary>
        Add = 1,

        /// <summary>乘法：FinalValue *= Value。按 Priority 从小到大依次累乘。</summary>
        Multiply = 2,

        /// <summary>最小值下限：FinalValue = Max(FinalValue, Value)。</summary>
        MinCap = 3,

        /// <summary>最大值上限：FinalValue = Min(FinalValue, Value)。</summary>
        MaxCap = 4,
    }

    /// <summary>
    /// 属性修正器，描述对一个属性值的单一修正操作。
    /// <typeparamref name="TId"/> 为来源标识类型（如 EntityId、技能 ID 等），解耦实体。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public readonly struct Modifier<TId> : IEquatable<Modifier<TId>>
        where TId : IEquatable<TId>
    {
        /// <summary>Modifier 来源标识（EntityId / 技能 ID / Buff ID 等）。</summary>
        public readonly TId Source;

        /// <summary>运算类型。</summary>
        public readonly ModifierOp Op;

        /// <summary>运算值（定点数）。</summary>
        public readonly Fixed64 Value;

        /// <summary>优先级（仅 <see cref="ModifierOp.Add"/> 和 <see cref="ModifierOp.Multiply"/> 有效，越小越优先计算）。</summary>
        public readonly int Priority;

        /// <summary>
        /// 初始化 Modifier。
        /// </summary>
        /// <param name="source">来源标识。</param>
        /// <param name="op">运算类型。</param>
        /// <param name="value">运算值。</param>
        /// <param name="priority">优先级，默认 0。仅 Add/Multiply 类型有效。</param>
        public Modifier(TId source, ModifierOp op, Fixed64 value, int priority = 0)
        {
            Source = source;
            Op = op;
            Value = value;
            Priority = priority;
        }

        public bool Equals(Modifier<TId> other) =>
            EqualityComparer<TId>.Default.Equals(Source, other.Source) &&
            Op == other.Op &&
            Value == other.Value &&
            Priority == other.Priority;

        public override bool Equals(object? obj) => obj is Modifier<TId> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Source, Op, Value, Priority);
        public static bool operator ==(Modifier<TId> left, Modifier<TId> right) => left.Equals(right);
        public static bool operator !=(Modifier<TId> left, Modifier<TId> right) => !left.Equals(right);
    }

    /// <summary>
    /// Modifier 句柄，用于跟踪和移除已添加的 Modifier。
    /// 由 <see cref="AttributeSet{TId}"/> 内部生成并返回给调用方。
    /// </summary>
    public readonly struct ModifierHandle : IEquatable<ModifierHandle>
    {
        /// <summary>内部 ID（从 1 开始递增，0 表示无效句柄）。</summary>
        internal readonly int Id;

        /// <summary>该句柄是否有效。</summary>
        public bool IsValid => Id > 0;

        internal ModifierHandle(int id) { Id = id; }

        /// <summary>无效句柄。</summary>
        public static readonly ModifierHandle Invalid = default;

        public bool Equals(ModifierHandle other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is ModifierHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(ModifierHandle left, ModifierHandle right) => left.Equals(right);
        public static bool operator !=(ModifierHandle left, ModifierHandle right) => !left.Equals(right);
    }
}
