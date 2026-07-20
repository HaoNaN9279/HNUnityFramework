#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 奖励句柄。用于追踪运行中的奖励发放实例。
    /// 与 <see cref="QuestHandle"/> 相同的 int-based struct 模式。
    /// </summary>
    public readonly struct RewardHandle : IEquatable<RewardHandle>
    {
        internal readonly int Id;

        /// <summary>句柄是否有效（Id > 0）。</summary>
        public bool IsValid => Id > 0;

        /// <summary>初始化奖励句柄（仅 RewardSystem 内部使用）。</summary>
        internal RewardHandle(int id) { Id = id; }

        /// <summary>无效句柄（默认值）。</summary>
        public static readonly RewardHandle Invalid = default;

        /// <inheritdoc />
        public bool Equals(RewardHandle other) => Id == other.Id;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is RewardHandle other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Id;

        /// <summary>相等运算符。</summary>
        public static bool operator ==(RewardHandle left, RewardHandle right) => left.Equals(right);

        /// <summary>不等运算符。</summary>
        public static bool operator !=(RewardHandle left, RewardHandle right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"RewardHandle({Id})";
    }
}
