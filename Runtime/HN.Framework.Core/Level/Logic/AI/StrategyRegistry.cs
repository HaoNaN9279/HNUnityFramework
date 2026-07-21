namespace HN.Framework.Core.Level.Logic.AI
{
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;
    using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

    /// <summary>
    /// 策略注册中心 — 注册、查询和管理所有可用决策策略
    /// 实现 IReference 支持引用池复用
    /// </summary>
    public class StrategyRegistry : IReference
    {
        private PooledDictionary<string, IDecisionStrategy> m_Strategies;

        /// <summary>
        /// 初始化注册中心，从引用池获取内部字典
        /// </summary>
        public void Initialize()
        {
            m_Strategies = ReferencePool.Acquire<PooledDictionary<string, IDecisionStrategy>>();
        }

        /// <summary>
        /// 注册策略（同名策略会覆盖旧注册）
        /// </summary>
        /// <param name="strategy">要注册的策略实例</param>
        public void Register(IDecisionStrategy strategy)
        {
            if (strategy != null && m_Strategies != null)
            {
                m_Strategies[strategy.Name] = strategy;
            }
        }

        /// <summary>
        /// 注销指定名称的策略
        /// </summary>
        /// <param name="name">要注销的策略名称</param>
        public void Unregister(string name)
        {
            m_Strategies?.Remove(name);
        }

        /// <summary>
        /// 按名称获取策略
        /// </summary>
        /// <param name="name">策略名称</param>
        /// <returns>找到的策略实例，未找到则返回 null</returns>
        public IDecisionStrategy Get(string name)
        {
            if (m_Strategies != null && m_Strategies.TryGetValue(name, out var strategy))
            {
                return strategy;
            }

            return null;
        }

        /// <summary>
        /// 是否包含指定名称的策略
        /// </summary>
        /// <param name="name">策略名称</param>
        /// <returns>存在则返回 true</returns>
        public bool Has(string name)
        {
            return m_Strategies != null && m_Strategies.ContainsKey(name);
        }

        /// <summary>
        /// 获取所有已注册策略（只读）
        /// </summary>
        /// <returns>已注册策略的只读集合</returns>
        public IReadOnlyCollection<IDecisionStrategy> GetAll()
        {
            return m_Strategies?.Values ?? (IReadOnlyCollection<IDecisionStrategy>)System.Array.Empty<IDecisionStrategy>();
        }

        /// <summary>
        /// 获取所有策略名称（只读）
        /// </summary>
        /// <returns>已注册策略名称的只读集合</returns>
        public IReadOnlyCollection<string> GetNames()
        {
            return m_Strategies?.Keys ?? (IReadOnlyCollection<string>)System.Array.Empty<string>();
        }

        /// <summary>
        /// 已注册策略数量
        /// </summary>
        public int Count
        {
            get
            {
                return m_Strategies?.Count ?? 0;
            }
        }

        /// <summary>
        /// 注销并重置所有已注册策略，但保留内部字典引用
        /// </summary>
        public void UnregisterAll()
        {
            if (m_Strategies != null)
            {
                foreach (var strategy in m_Strategies.Values)
                {
                    strategy.Reset();
                }

                m_Strategies.Clear();
            }
        }

        /// <summary>
        /// 实现 IReference.Clear — 释放内部字典到引用池
        /// 不同于 UnregisterAll，此方法会将字典本身归还引用池
        /// </summary>
        public void Clear()
        {
            if (m_Strategies != null)
            {
                // 先重置所有注册的策略
                foreach (var strategy in m_Strategies.Values)
                {
                    strategy.Reset();
                }

                ReferencePool.Release(m_Strategies);
                m_Strategies = null;
            }
        }
    }
}
