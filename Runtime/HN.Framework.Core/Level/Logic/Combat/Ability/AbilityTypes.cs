#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 技能规格定义。描述一个技能的完整数据，通常来自配置表。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class AbilitySpec<TId> where TId : IEquatable<TId>
    {
        /// <summary>技能定义 ID。</summary>
        public int AbilityId { get; set; }

        /// <summary>技能名称。</summary>
        public string? DisplayName { get; set; }

        /// <summary>冷却时间（秒）。</summary>
        public Fixed64 Cooldown { get; set; }

        /// <summary>技能消耗（属性类型 → 消耗量）。</summary>
        public Dictionary<AttributeType, Fixed64>? Cost { get; set; }

        /// <summary>激活所需标签条件（TagQuery 的序列化节点，由外部生成）。</summary>
        /// <remarks>为 null 或空表示无标签条件。</remarks>
        public GameplayTagQueryData? ActivationTagQuery { get; set; }

        /// <summary>阻止激活的标签条件（如眩晕时不能使用技能）。</summary>
        public GameplayTagQueryData? BlockingTagQuery { get; set; }

        /// <summary>技能激活时应用的效果列表。</summary>
        public List<EffectSpec<TId>> Effects { get; set; } = new();

        /// <summary>技能范围（预留字段）。</summary>
        public float Range { get; set; }

        /// <summary>技能目标类型：自身 / 敌方 / 友方 / 位置。</summary>
        public AbilityTargetType TargetType { get; set; } = AbilityTargetType.Enemy;
    }

    /// <summary>
    /// 技能目标类型。
    /// </summary>
    public enum AbilityTargetType : byte
    {
        /// <summary>自身。</summary>
        Self = 0,
        /// <summary>敌方。</summary>
        Enemy = 1,
        /// <summary>友方。</summary>
        Ally = 2,
        /// <summary>位置。</summary>
        Position = 3,
    }

    /// <summary>
    /// 可序列化的 GameplayTag 查询条件数据。
    /// 用于存储 TagQuery 的扁平节点数组以便配置表序列化。
    /// </summary>
    public sealed class GameplayTagQueryData
    {
        /// <summary>查询节点的扁平数组（TagQuery 内部格式）。</summary>
        public int[]? FlatNodes { get; set; }

        /// <summary>是否为空查询（始终匹配）。</summary>
        public bool IsEmpty => FlatNodes == null || FlatNodes.Length == 0;
    }

    /// <summary>
    /// 技能运行时实例。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    internal sealed class AbilityInstance<TId> where TId : IEquatable<TId>
    {
        public readonly AbilitySpec<TId> Spec;
        public readonly TId Owner;
        public Fixed64 CooldownRemaining;
        public bool IsOnCooldown => CooldownRemaining > Fixed64.Zero;
        public bool IsExpired;

        public AbilityInstance(AbilitySpec<TId> spec, TId owner)
        {
            Spec = spec;
            Owner = owner;
            CooldownRemaining = Fixed64.Zero;
        }
    }

    /// <summary>
    /// 技能句柄。
    /// </summary>
    public readonly struct AbilityHandle : IEquatable<AbilityHandle>
    {
        internal readonly int Id;
        public bool IsValid => Id > 0;
        internal AbilityHandle(int id) { Id = id; }
        public static readonly AbilityHandle Invalid = default;

        public bool Equals(AbilityHandle other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is AbilityHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(AbilityHandle left, AbilityHandle right) => left.Equals(right);
        public static bool operator !=(AbilityHandle left, AbilityHandle right) => !left.Equals(right);
    }
}
