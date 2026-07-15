#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level
{
    /// <summary>
    /// 标签容器，管理一组 <see cref="GameplayTag"/>，提供增删查和批量匹配操作。
    /// 内部使用 <see cref="HashSet{T}"/> 存储 TableIndex 值。
    /// </summary>
    public class GameplayTagContainer
    {
        private readonly HashSet<int> _tags;

        /// <summary>
        /// 初始化空的标签容器。
        /// </summary>
        public GameplayTagContainer()
        {
            _tags = new HashSet<int>();
        }

        /// <summary>
        /// 拷贝构造：从另一个标签容器复制所有标签。
        /// </summary>
        /// <param name="other">要复制的源容器。不可为 <c>null</c>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> 为 <c>null</c> 时抛出。</exception>
        public GameplayTagContainer(GameplayTagContainer other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            _tags = new HashSet<int>(other._tags);
        }

        /// <summary>
        /// 容器中的标签数量。
        /// </summary>
        public int Count => _tags.Count;

        /// <summary>
        /// 添加标签。若标签无效或已存在则不重复添加。
        /// </summary>
        /// <param name="tag">要添加的 GameplayTag。</param>
        public void AddTag(GameplayTag tag)
        {
            if (tag.IsValid)
            {
                _tags.Add(tag.TableIndex);
            }
        }

        /// <summary>
        /// 移除标签。仅移除严格匹配的 TableIndex。
        /// 若标签无效或不存在则静默忽略。
        /// </summary>
        /// <param name="tag">要移除的 GameplayTag。</param>
        public void RemoveTag(GameplayTag tag)
        {
            if (tag.IsValid)
            {
                _tags.Remove(tag.TableIndex);
            }
        }

        /// <summary>
        /// 清空所有标签。
        /// </summary>
        public void Clear()
        {
            _tags.Clear();
        }

        /// <summary>
        /// 层级匹配：检查是否存在指定标签或其子标签。
        /// 遍历容器中的所有 TableIndex，对每个构造临时 GameplayTag 调用 <see cref="GameplayTag.Matches"/>，
        /// 检查其是否属于 <paramref name="tag"/> 的层级子树。
        /// </summary>
        /// <param name="tag">要匹配的祖先标签。</param>
        /// <returns>如果容器中存在 <paramref name="tag"/> 或其后代标签，则返回 <c>true</c>。</returns>
        /// <example>
        /// 容器中有 <c>State.Combat.Stunned</c>（TableIndex=5），调用 <c>HasTag(State)</c>（TableIndex=1）：
        /// 由于 Stunned 是 State 的后代，返回 <c>true</c>。
        /// </example>
        public bool HasTag(GameplayTag tag)
        {
            if (!tag.IsValid)
            {
                return false;
            }

            foreach (var tableIndex in _tags)
            {
                var t = new GameplayTag(tableIndex, 0);
                if (t.Matches(tag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 严格匹配：检查是否存在 TableIndex 完全相等的标签。
        /// 仅比较 TableIndex 值，不进行层级遍历。
        /// </summary>
        /// <param name="tag">要匹配的标签。</param>
        /// <returns>如果容器中存在严格匹配的标签，则返回 <c>true</c>。</returns>
        public bool HasTagExact(GameplayTag tag)
        {
            if (!tag.IsValid)
            {
                return false;
            }

            return _tags.Contains(tag.TableIndex);
        }

        /// <summary>
        /// 检查容器是否包含 <paramref name="other"/> 中的任意一个标签（层级匹配）。
        /// 只要存在一个匹配即返回 <c>true</c>。
        /// </summary>
        /// <param name="other">要检查的目标容器。</param>
        /// <returns>如果当前容器至少包含一个 <paramref name="other"/> 中的标签（或其子标签），则返回 <c>true</c>。</returns>
        public bool HasAny(GameplayTagContainer other)
        {
            if (other == null)
            {
                return false;
            }

            foreach (var tableIndex in other._tags)
            {
                if (HasTag(new GameplayTag(tableIndex, 0)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查容器是否包含 <paramref name="other"/> 中所有的标签（层级匹配）。
        /// 所有标签都必须有匹配才返回 <c>true</c>。
        /// </summary>
        /// <param name="other">要检查的目标容器。</param>
        /// <returns>如果当前容器包含 <paramref name="other"/> 中的所有标签（或其子标签），则返回 <c>true</c>。</returns>
        public bool HasAll(GameplayTagContainer other)
        {
            if (other == null)
            {
                return false;
            }

            foreach (var tableIndex in other._tags)
            {
                if (!HasTag(new GameplayTag(tableIndex, 0)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 使用 <see cref="TagQuery"/> 表达式评估当前容器。
        /// 将当前容器委托给 TagQuery 的 <c>Matches</c> 方法进行匹配判断。
        /// </summary>
        /// <param name="query">要评估的 TagQuery 表达式。不可为 <c>null</c>。</param>
        /// <returns>如果标签集合满足查询条件，则返回 <c>true</c>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="query"/> 为 <c>null</c> 时抛出。</exception>
        public bool MatchesQuery(TagQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query.Matches(this);
        }

        /// <summary>
        /// 批量添加另一个容器的所有标签。
        /// </summary>
        /// <param name="other">要添加的源容器。不可为 <c>null</c>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> 为 <c>null</c> 时抛出。</exception>
        public void AddTags(GameplayTagContainer other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            foreach (var tableIndex in other._tags)
            {
                _tags.Add(tableIndex);
            }
        }

        /// <summary>
        /// 批量移除另一个容器的所有标签。使用 TableIndex 严格匹配移除。
        /// </summary>
        /// <param name="other">要移除的源容器。不可为 <c>null</c>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> 为 <c>null</c> 时抛出。</exception>
        public void RemoveTags(GameplayTagContainer other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            foreach (var tableIndex in other._tags)
            {
                _tags.Remove(tableIndex);
            }
        }

        /// <summary>
        /// 获取序列化用的标签 TableIndex 数组（仅供序列化器使用）。
        /// </summary>
        internal int[] GetTagsForSerialization()
        {
            var result = new int[_tags.Count];
            _tags.CopyTo(result);
            return result;
        }

        /// <summary>
        /// 从标签 TableIndex 数组填充容器（仅供反序列化器使用）。
        /// </summary>
        /// <param name="tags">标签 TableIndex 数组。</param>
        internal void SetTagsFromDeserialization(int[] tags)
        {
            _tags.Clear();
            foreach (var t in tags)
            {
                _tags.Add(t);
            }
        }
    }
}
