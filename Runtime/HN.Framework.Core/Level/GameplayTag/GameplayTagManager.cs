#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level
{
    /// <summary>
    /// 管理 GameplayTag 定义表的生命周期。从配置表加载定义，建立 string↔index 映射，冻结后不可变更。
    /// </summary>
    public class GameplayTagManager
    {
        private GameplayTagDefinition[] _definitions = Array.Empty<GameplayTagDefinition>();
        private Dictionary<string, int> _nameToIndex = new Dictionary<string, int>();

        /// <summary>
        /// 当前静态实例。由 GameWorld 在初始化时设置。
        /// GameplayTag struct 通过此属性访问定义表进行层级匹配。
        /// </summary>
        internal static GameplayTagManager? Current { get; private set; }

        /// <summary>
        /// 是否已冻结。冻结后禁止加载或修改。
        /// </summary>
        public bool IsFrozen { get; private set; }

        /// <summary>
        /// 已注册的标签数量。
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 从定义列表加载标签。加载后自动构建名称映射表。
        /// </summary>
        /// <param name="definitions">标签定义列表，按深度优先排序（父节点在前，子节点在后）。</param>
        /// <exception cref="InvalidOperationException">已冻结时调用。</exception>
        /// <exception cref="ArgumentNullException">definitions 为 null。</exception>
        public void LoadFromDefinitions(IList<GameplayTagDefinition> definitions)
        {
            if (IsFrozen)
            {
                throw new InvalidOperationException("GameplayTagManager 已冻结，禁止加载定义。");
            }

            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            // 内部存储原始定义（0-based），但对外暴露的 TableIndex 使用 1-based
            // TableIndex=0 保留给 GameplayTag.Empty（无效标签）
            _definitions = new GameplayTagDefinition[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                _definitions[i] = definitions[i];
            }

            // 名称→TableIndex 映射：偏移 +1，使 TableIndex 从 1 开始
            _nameToIndex = new Dictionary<string, int>(_definitions.Length);
            for (int i = 0; i < _definitions.Length; i++)
            {
                _nameToIndex[_definitions[i].FullName] = i + 1;
            }

            Current = null;
            Count = definitions.Count;
        }

        /// <summary>
        /// 冻结管理器，禁止后续修改。通常在加载完成后调用。
        /// </summary>
        public void Freeze()
        {
            IsFrozen = true;
            Current = this;
        }

        /// <summary>
        /// 通过完整名称查找标签。
        /// </summary>
        /// <param name="fullName">如 "State.Combat.Stunned"。</param>
        /// <returns>对应的 GameplayTag（TableIndex 从 1 开始，0 保留给 Empty）。</returns>
        /// <exception cref="KeyNotFoundException">名称未注册时抛出。</exception>
        public GameplayTag GetTag(string fullName)
        {
            if (_nameToIndex.TryGetValue(fullName, out int index))
            {
                return new GameplayTag(index, 0);
            }

            throw new KeyNotFoundException($"未找到名为 \"{fullName}\" 的 GameplayTag。");
        }

        /// <summary>
        /// 尝试通过完整名称查找标签。
        /// </summary>
        /// <param name="fullName">要查找的标签完整名称。</param>
        /// <param name="tag">找到的 GameplayTag（未找到时为 Empty，TableIndex 从 1 开始）。</param>
        /// <returns>如果找到则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public bool TryGetTag(string fullName, out GameplayTag tag)
        {
            if (_nameToIndex.TryGetValue(fullName, out int index))
            {
                tag = new GameplayTag(index, 0);
                return true;
            }

            tag = GameplayTag.Empty;
            return false;
        }

        /// <summary>
        /// 获取标签的层级深度。
        /// </summary>
        /// <param name="tag">要查询的 GameplayTag（TableIndex 从 1 开始）。</param>
        /// <returns>标签的层级深度（从 1 开始）。</returns>
        /// <exception cref="ArgumentException">tag 无效或索引越界时抛出。</exception>
        public int GetTagDepth(GameplayTag tag)
        {
            if (!tag.IsValid)
            {
                throw new ArgumentException("无法获取无效标签的深度。", nameof(tag));
            }

            // TableIndex 是 1-based，内部存储是 0-based
            int internalIndex = tag.TableIndex - 1;
            if ((uint)internalIndex >= (uint)_definitions.Length)
            {
                throw new ArgumentException($"标签索引 {tag.TableIndex} 超出定义表范围。", nameof(tag));
            }

            return _definitions[internalIndex].Depth;
        }

        /// <summary>
        /// 检查 tag 是否是 potentialAncestor 的后代。
        /// 通过遍历 tag 的 ParentIndex 链实现。
        /// 内部 ParentIndex 为 0-based，GameplayTag.TableIndex 为 1-based，
        /// 比较时做偏移转换确保正确匹配。
        /// </summary>
        internal static bool IsDescendantOf(GameplayTag tag, GameplayTag potentialAncestor)
        {
            var mgr = Current;
            if (mgr == null || !tag.IsValid || !potentialAncestor.IsValid)
            {
                return false;
            }

            var definitions = mgr._definitions;
            if (definitions == null)
            {
                return false;
            }

            // TableIndex 是 1-based，内部定义是 0-based
            int internalIndex = tag.TableIndex - 1;
            if ((uint)internalIndex >= (uint)definitions.Length)
            {
                return false;
            }

            var current = definitions[internalIndex];
            // potentialAncestor 的 TableIndex 转为 0-based 以便与 ParentIndex 比较
            int ancestorInternalIndex = potentialAncestor.TableIndex - 1;

            while (current.ParentIndex >= 0)
            {
                if (current.ParentIndex == ancestorInternalIndex)
                {
                    return true;
                }

                if ((uint)current.ParentIndex >= (uint)definitions.Length)
                {
                    return false;
                }

                current = definitions[current.ParentIndex];
            }

            return false;
        }

        /// <summary>
        /// 获取标签名称的静态辅助方法（供 GameplayTag.ToString 使用）。
        /// 当 Current 未设置或标签无效时返回 <c>null</c>。
        /// </summary>
        internal static string? GetTagName(GameplayTag tag)
        {
            var mgr = Current;
            if (mgr == null || !tag.IsValid)
            {
                return null;
            }

            var definitions = mgr._definitions;
            // TableIndex 是 1-based，内部存储是 0-based
            int internalIndex = tag.TableIndex - 1;
            if (definitions == null || (uint)internalIndex >= (uint)definitions.Length)
            {
                return null;
            }

            return definitions[internalIndex].FullName;
        }

        /// <summary>
        /// 标签定义，描述一个层级标签的元数据。
        /// </summary>
        public class GameplayTagDefinition
        {
            /// <summary>完整层级名称，如 "State.Combat.Stunned"。</summary>
            public string FullName { get; set; } = string.Empty;

            /// <summary>自身在定义表中的索引。</summary>
            public int Index { get; set; }

            /// <summary>父标签索引（-1 表示根节点）。</summary>
            public int ParentIndex { get; set; } = -1;

            /// <summary>层级深度（从 1 开始）。</summary>
            public int Depth { get; set; }
        }
    }
}
