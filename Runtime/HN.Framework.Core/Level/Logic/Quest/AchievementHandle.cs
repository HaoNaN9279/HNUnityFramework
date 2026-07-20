#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 成就句柄。用于追踪运行中的成就实例。
    /// 与 <see cref="QuestHandle"/> 相同的 int-based struct 模式。
    /// </summary>
    public readonly struct AchievementHandle : IEquatable<AchievementHandle>
    {
        internal readonly int Id;

        /// <summary>句柄是否有效（Id > 0）。</summary>
        public bool IsValid => Id > 0;

        /// <summary>初始化成就句柄（仅 AchievementSystem 内部使用）。</summary>
        internal AchievementHandle(int id) { Id = id; }

        /// <summary>无效句柄（默认值）。</summary>
        public static readonly AchievementHandle Invalid = default;

        /// <inheritdoc />
        public bool Equals(AchievementHandle other) => Id == other.Id;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is AchievementHandle other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Id;

        /// <summary>相等运算符。</summary>
        public static bool operator ==(AchievementHandle left, AchievementHandle right) => left.Equals(right);

        /// <summary>不等运算符。</summary>
        public static bool operator !=(AchievementHandle left, AchievementHandle right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"AchievementHandle({Id})";
    }
}
