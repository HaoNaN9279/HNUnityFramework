using System;

namespace HN.Framework.Core.Driver.Common.Pool.ObjectPool
{
    /// <summary>
    /// 对象池配置参数
    /// </summary>
    public struct PoolSettings
    {
        /// <summary>
        /// 对象池名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 初始数量（创建时自动填充）
        /// </summary>
        public int InitialCount;

        /// <summary>
        /// Tick 频率（每隔 N 帧触发一次 Tick）
        /// </summary>
        public int TickFrequency;

        /// <summary>
        /// 最大数量（超过此值时 Tick 销毁一个）
        /// </summary>
        public int MaxCount;

        /// <summary>
        /// 最小数量（低于此值时 Tick 创建一个）
        /// </summary>
        public int MinCount;

        /// <summary>
        /// 最大极限数量（超过此值时 Tick 销毁至 MaxCount）
        /// </summary>
        public int MaxLimitCount;

        /// <summary>
        /// 最小极限数量（低于此值时 Tick 创建至 MinCount）
        /// </summary>
        public int MinLimitCount;

        /// <summary>
        /// 返回默认配置
        /// </summary>
        /// <param name="name">对象池名称</param>
        /// <returns>默认配置</returns>
        public static PoolSettings Default(string name) => new PoolSettings
        {
            Name = name,
            InitialCount = 0,
            TickFrequency = 0,
            MaxCount = int.MaxValue,
            MinCount = 0,
            MaxLimitCount = int.MaxValue,
            MinLimitCount = 0,
        };
    }
}
