using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic.AI.KnowledgePool
{
    /// <summary>
    /// 黑板 — Agent 内部上下文数据共享
    /// 线程不安全，设计为单线程使用
    /// </summary>
    public class Blackboard : IReference
    {
        private PooledDictionary<string, object> m_Data;
        private static readonly Dictionary<string, object> s_EmptyData = new Dictionary<string, object>(0);

        /// <summary>
        /// 初始化黑板
        /// </summary>
        public void Initialize()
        {
            m_Data = ReferencePool.Acquire<PooledDictionary<string, object>>();
        }

        /// <summary>
        /// 设置黑板值
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <typeparam name="T">值类型</typeparam>
        public void Set<T>(string key, T value)
        {
            if (m_Data == null)
            {
                return;
            }

            m_Data[key] = value;
        }

        /// <summary>
        /// 获取黑板值
        /// </summary>
        /// <param name="key">键名</param>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>值，不存在则返回 default(T)</returns>
        public T Get<T>(string key)
        {
            if (m_Data == null)
            {
                return default;
            }

            if (m_Data.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// 尝试获取黑板值
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">输出值</param>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>是否存在该键</returns>
        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (m_Data == null)
            {
                return false;
            }

            if (m_Data.TryGetValue(key, out object obj))
            {
                value = (T)obj;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否包含指定键
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>是否存在</returns>
        public bool HasKey(string key)
        {
            return m_Data != null && m_Data.ContainsKey(key);
        }

        /// <summary>
        /// 移除指定键
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>是否成功移除</returns>
        public bool Remove(string key)
        {
            return m_Data != null && m_Data.Remove(key);
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAll()
        {
            if (m_Data != null)
            {
                m_Data.Clear();
            }
        }

        /// <summary>
        /// 黑板中数据项数量
        /// </summary>
        public int Count => m_Data?.Count ?? 0;

        /// <summary>
        /// 获取所有键
        /// </summary>
        public Dictionary<string, object>.KeyCollection Keys => m_Data?.Keys ?? s_EmptyData.Keys;

        /// <summary>
        /// 清理并释放资源
        /// </summary>
        public void Clear()
        {
            if (m_Data != null)
            {
                ReferencePool.Release(m_Data);
                m_Data = null;
            }
        }
    }
}
