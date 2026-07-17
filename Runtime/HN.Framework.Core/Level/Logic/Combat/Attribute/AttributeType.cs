#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 属性类型值类型，使用 int 索引标识，零字符串运行时开销。
    /// 与 <see cref="GameplayTag"/> 相同的 int-based struct 模式。
    /// 由 <see cref="AttributeTypeManager"/> 从配置表加载并管理生命周期。
    /// </summary>
    public readonly struct AttributeType : IEquatable<AttributeType>
    {
        private readonly int _index;

        /// <summary>内部索引。0 表示无效类型，有效索引从 1 开始。</summary>
        public int Index => _index;

        /// <summary>空/无效属性类型（默认值）。</summary>
        public static readonly AttributeType Empty = default;

        /// <summary>当前类型是否有效（Index != 0）。</summary>
        public bool IsValid => _index != 0;

        /// <summary>
        /// 初始化 AttributeType 实例（仅 <see cref="AttributeTypeManager"/> 内部使用）。
        /// </summary>
        /// <param name="index">定义表中的索引位置（1-based）。</param>
        internal AttributeType(int index)
        {
            _index = index;
        }

        /// <inheritdoc />
        public bool Equals(AttributeType other) => _index == other._index;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is AttributeType other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _index;

        /// <summary>相等运算符。</summary>
        public static bool operator ==(AttributeType left, AttributeType right) => left.Equals(right);

        /// <summary>不等运算符。</summary>
        public static bool operator !=(AttributeType left, AttributeType right) => !left.Equals(right);

        /// <summary>
        /// 返回属性类型名称（调试用）。
        /// 未初始化时返回 "Empty"，未找到名称时返回 "AttributeType({Index})"。
        /// </summary>
        public override string ToString()
        {
            if (!IsValid) return "Empty";
            var name = AttributeTypeManager.GetTypeName(this);
            return string.IsNullOrEmpty(name) ? $"AttributeType({_index})" : name;
        }
    }

    /// <summary>
    /// 管理属性类型定义的生命周期。从配置表加载定义，建立 string↔index 映射，冻结后不可变更。
    /// 与 <see cref="GameplayTagManager"/> 相同模式。
    /// </summary>
    public class AttributeTypeManager
    {
        private AttributeDefinition[] _definitions = Array.Empty<AttributeDefinition>();
        private Dictionary<string, int> _nameToIndex = new Dictionary<string, int>();

        /// <summary>
        /// 当前静态实例。由 GameWorld 在初始化时设置。
        /// <see cref="AttributeType"/> struct 通过此属性访问定义表进行名称解析。
        /// </summary>
        internal static AttributeTypeManager? Current { get; private set; }

        /// <summary>是否已冻结。冻结后禁止加载或修改。</summary>
        public bool IsFrozen { get; private set; }

        /// <summary>已注册的属性类型数量。</summary>
        public int Count { get; private set; }

        /// <summary>
        /// 从定义列表加载属性类型。加载后自动构建名称映射表。
        /// </summary>
        /// <param name="definitions">属性类型定义列表。</param>
        /// <exception cref="InvalidOperationException">已冻结时调用。</exception>
        /// <exception cref="ArgumentNullException">definitions 为 null。</exception>
        public void LoadFromDefinitions(IList<AttributeDefinition> definitions)
        {
            if (IsFrozen)
                throw new InvalidOperationException("AttributeTypeManager 已冻结，禁止加载定义。");
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            // 内部存储原始定义（0-based），对外暴露的 Index 使用 1-based
            // Index=0 保留给 AttributeType.Empty（无效类型）
            _definitions = new AttributeDefinition[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                _definitions[i] = definitions[i];
            }

            _nameToIndex = new Dictionary<string, int>(_definitions.Length);
            for (int i = 0; i < _definitions.Length; i++)
            {
                _nameToIndex[_definitions[i].Name] = i + 1;
            }

            Current = null;
            Count = definitions.Count;
        }

        /// <summary>冻结管理器，禁止后续修改。通常在加载完成后调用。</summary>
        public void Freeze()
        {
            IsFrozen = true;
            Current = this;
        }

        /// <summary>
        /// 通过名称查找属性类型。
        /// </summary>
        /// <param name="name">属性类型名称，如 "MaxHP"。</param>
        /// <returns>对应的 AttributeType（Index 从 1 开始，0 保留给 Empty）。</returns>
        /// <exception cref="KeyNotFoundException">名称未注册时抛出。</exception>
        public AttributeType GetType(string name)
        {
            if (_nameToIndex.TryGetValue(name, out int index))
                return new AttributeType(index);
            throw new KeyNotFoundException($"未找到名为 \"{name}\" 的 AttributeType。");
        }

        /// <summary>尝试通过名称查找属性类型。</summary>
        /// <param name="name">要查找的类型名称。</param>
        /// <param name="type">找到的 AttributeType（未找到时为 Empty）。</param>
        /// <returns>如果找到则返回 true，否则 false。</returns>
        public bool TryGetType(string name, out AttributeType type)
        {
            if (_nameToIndex.TryGetValue(name, out int index))
            {
                type = new AttributeType(index);
                return true;
            }
            type = AttributeType.Empty;
            return false;
        }

        /// <summary>获取属性类型的名称。</summary>
        internal static string? GetTypeName(AttributeType type)
        {
            var mgr = Current;
            if (mgr == null || !type.IsValid) return null;

            // Index 是 1-based，内部存储是 0-based
            int internalIndex = type.Index - 1;
            var defs = mgr._definitions;
            if (defs == null || (uint)internalIndex >= (uint)defs.Length) return null;

            return defs[internalIndex].Name;
        }

        /// <summary>获取属性类型的定义信息。</summary>
        internal static AttributeDefinition? GetDefinition(AttributeType type)
        {
            var mgr = Current;
            if (mgr == null || !type.IsValid) return null;

            int internalIndex = type.Index - 1;
            var defs = mgr._definitions;
            if (defs == null || (uint)internalIndex >= (uint)defs.Length) return null;

            return defs[internalIndex];
        }
    }
}
