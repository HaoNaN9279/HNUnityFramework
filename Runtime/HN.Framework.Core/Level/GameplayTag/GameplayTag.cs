#nullable enable

using System;

namespace HN.Framework.Core.Level
{
    /// <summary>
    /// 层级标签值类型，使用 TableIndex + InstanceId 索引编码。
    /// 无层级深度限制，层级匹配通过 ParentIndex 链遍历实现。
    /// 零字符串运行时开销。
    /// </summary>
    public readonly struct GameplayTag : IEquatable<GameplayTag>
    {
        private readonly int _tableIndex;
        private readonly int _instanceId;

        /// <summary>定义表索引。0 表示无效标签。</summary>
        public int TableIndex => _tableIndex;

        /// <summary>实例 ID（保留字段，目前恒为 0）。</summary>
        public int InstanceId => _instanceId;

        /// <summary>空标签（默认值）。</summary>
        public static readonly GameplayTag Empty = default;

        /// <summary>当前标签是否有效（TableIndex != 0）。</summary>
        public bool IsValid => _tableIndex != 0;

        /// <summary>
        /// 初始化 GameplayTag 实例（仅 GameplayTagManager 内部使用）。
        /// </summary>
        /// <param name="tableIndex">定义表中的索引位置。</param>
        /// <param name="instanceId">实例 ID（保留字段）。</param>
        internal GameplayTag(int tableIndex, int instanceId)
        {
            _tableIndex = tableIndex;
            _instanceId = instanceId;
        }

        /// <summary>
        /// 层级匹配：检查当前标签是否是 <paramref name="other"/> 标签或其后代节点。
        /// 例如 tag(State.Combat.Stunned).Matches(tag(State)) → true。
        /// 依赖 GameplayTagManager 初始化完成后才可正常工作。
        /// </summary>
        /// <param name="other">要匹配的祖先标签。</param>
        /// <returns>如果当前标签是 <paramref name="other"/> 或其子标签，则返回 <c>true</c>。</returns>
        public bool Matches(GameplayTag other)
        {
            if (_tableIndex == other._tableIndex)
            {
                return true;
            }

            if (!other.IsValid)
            {
                return false;
            }

            return GameplayTagManager.IsDescendantOf(this, other);
        }

        /// <summary>
        /// 严格相等匹配：仅比较 TableIndex。
        /// </summary>
        /// <param name="other">要比较的标签。</param>
        /// <returns>如果 TableIndex 相等则返回 <c>true</c>。</returns>
        public bool MatchesExact(GameplayTag other) => _tableIndex == other._tableIndex;

        /// <inheritdoc />
        public bool Equals(GameplayTag other) => _tableIndex == other._tableIndex;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is GameplayTag other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _tableIndex;

        /// <summary>
        /// 相等运算符。
        /// </summary>
        public static bool operator ==(GameplayTag left, GameplayTag right) => left.Equals(right);

        /// <summary>
        /// 不等运算符。
        /// </summary>
        public static bool operator !=(GameplayTag left, GameplayTag right) => !left.Equals(right);

        /// <summary>
        /// 返回标签名称（调试用）。
        /// 未初始化时返回 "Empty"，未找到名称时返回 "GameplayTag({TableIndex})"。
        /// </summary>
        /// <returns>标签的可读名称字符串。</returns>
        public override string ToString()
        {
            if (!IsValid)
            {
                return "Empty";
            }

            var name = GameplayTagManager.GetTagName(this);
            return string.IsNullOrEmpty(name) ? $"GameplayTag({_tableIndex})" : name;
        }
    }
}
