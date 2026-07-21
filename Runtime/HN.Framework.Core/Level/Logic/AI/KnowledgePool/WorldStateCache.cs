namespace HN.Framework.Core.Level.Logic.AI.KnowledgePool
{
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

    /// <summary>
    /// 世界状态缓存 — 存储 Agent 感知到的环境数据和运行时上下文。
    /// 与 Blackboard 的不同之处在于：WorldState 代表外部世界的感知数据快照（随时间刷新），
    /// 而 Blackboard 是 Agent 自身的内部状态存储。
    /// 每帧刷新，过期数据自动失效。
    /// </summary>
    public class WorldStateCache : IReference
    {
        private PooledDictionary<string, WorldStateEntry> m_States;
        private float m_CurrentTime;

        /// <summary>
        /// 初始化世界状态缓存，从引用池获取内部存储字典。
        /// </summary>
        public void Initialize()
        {
            m_States = ReferencePool.Acquire<PooledDictionary<string, WorldStateEntry>>();
            m_CurrentTime = 0f;
        }

        /// <summary>
        /// 设置世界状态值（不过期）。
        /// </summary>
        /// <typeparam name="T">状态值类型。</typeparam>
        /// <param name="key">状态键。</param>
        /// <param name="value">状态值。</param>
        public void Set<T>(string key, T value)
        {
            Set(key, value, -1f);
        }

        /// <summary>
        /// 设置世界状态值（带过期时间）。
        /// </summary>
        /// <typeparam name="T">状态值类型。</typeparam>
        /// <param name="key">状态键。</param>
        /// <param name="value">状态值。</param>
        /// <param name="lifetime">生命周期（秒），-1 表示不过期。</param>
        public void Set<T>(string key, T value, float lifetime)
        {
            if (m_States == null)
            {
                return;
            }

            float expiryTime = lifetime > 0f ? m_CurrentTime + lifetime : -1f;
            m_States[key] = new WorldStateEntry { Value = value, ExpiryTime = expiryTime };
        }

        /// <summary>
        /// 获取世界状态值。
        /// </summary>
        /// <typeparam name="T">期望的返回值类型。</typeparam>
        /// <param name="key">状态键。</param>
        /// <returns>值，不存在或已过期返回 default。</returns>
        public T Get<T>(string key)
        {
            if (m_States == null || !m_States.TryGetValue(key, out WorldStateEntry entry))
            {
                return default;
            }

            if (entry.ExpiryTime > 0f && m_CurrentTime > entry.ExpiryTime)
            {
                m_States.Remove(key);
                return default;
            }

            return (T)entry.Value;
        }

        /// <summary>
        /// 尝试获取世界状态值。
        /// </summary>
        /// <typeparam name="T">期望的返回值类型。</typeparam>
        /// <param name="key">状态键。</param>
        /// <param name="value">输出值，成功时有效。</param>
        /// <returns>是否存在有效值。</returns>
        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (m_States == null || !m_States.TryGetValue(key, out WorldStateEntry entry))
            {
                return false;
            }

            if (entry.ExpiryTime > 0f && m_CurrentTime > entry.ExpiryTime)
            {
                m_States.Remove(key);
                return false;
            }

            value = (T)entry.Value;
            return true;
        }

        /// <summary>
        /// 是否包含指定键的有效状态值（不过期或未过期）。
        /// </summary>
        /// <param name="key">状态键。</param>
        /// <returns>存在且未过期返回 true。</returns>
        public bool HasKey(string key)
        {
            if (m_States == null)
            {
                return false;
            }

            if (!m_States.TryGetValue(key, out WorldStateEntry entry))
            {
                return false;
            }

            if (entry.ExpiryTime > 0f && m_CurrentTime > entry.ExpiryTime)
            {
                m_States.Remove(key);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 移除指定键的状态。
        /// </summary>
        /// <param name="key">状态键。</param>
        /// <returns>移除成功返回 true。</returns>
        public bool Remove(string key)
        {
            return m_States != null && m_States.Remove(key);
        }

        /// <summary>
        /// 清空所有状态（不释放内部字典）。
        /// </summary>
        public void ClearAll()
        {
            if (m_States != null)
            {
                m_States.Clear();
            }
        }

        /// <summary>
        /// 更新全局时间（每帧调用），用于过期判定。
        /// </summary>
        /// <param name="deltaTime">帧增量时间（秒）。</param>
        public void Tick(float deltaTime)
        {
            m_CurrentTime += deltaTime;
        }

        /// <summary>
        /// 当前缓存中的状态数量。
        /// </summary>
        public int Count
        {
            get
            {
                return m_States?.Count ?? 0;
            }
        }

        /// <summary>
        /// 清理资源，回收内部字典到引用池。
        /// </summary>
        public void Clear()
        {
            if (m_States != null)
            {
                ReferencePool.Release(m_States);
                m_States = null;
            }

            m_CurrentTime = 0f;
        }

        /// <summary>
        /// 世界状态条目，存储值和过期时间戳。
        /// </summary>
        private struct WorldStateEntry
        {
            /// <summary>
            /// 状态值。
            /// </summary>
            public object Value;

            /// <summary>
            /// 过期时间（绝对时间戳），-1 表示不过期。
            /// </summary>
            public float ExpiryTime;
        }
    }
}
