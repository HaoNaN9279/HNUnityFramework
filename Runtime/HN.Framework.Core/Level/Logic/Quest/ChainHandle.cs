#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务链句柄。用于追踪运行中的任务链实例。
    /// 与 <see cref="QuestHandle"/> 相同的 int-based struct 模式。
    /// </summary>
    public readonly struct ChainHandle : IEquatable<ChainHandle>
    {
        internal readonly int Id;

        /// <summary>句柄是否有效（Id > 0）。</summary>
        public bool IsValid => Id > 0;

        /// <summary>初始化任务链句柄（仅 QuestChainSystem 内部使用）。</summary>
        internal ChainHandle(int id) { Id = id; }

        /// <summary>无效句柄（默认值）。</summary>
        public static readonly ChainHandle Invalid = default;

        /// <inheritdoc />
        public bool Equals(ChainHandle other) => Id == other.Id;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ChainHandle other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Id;

        /// <summary>相等运算符。</summary>
        public static bool operator ==(ChainHandle left, ChainHandle right) => left.Equals(right);

        /// <summary>不等运算符。</summary>
        public static bool operator !=(ChainHandle left, ChainHandle right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"ChainHandle({Id})";
    }
}
