using System;
using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Core.Driver.Common
{
    /// <summary>
    /// 对象池抽象基类
    /// </summary>
    public abstract class PoolBase : ITickable, IReference
    {
        /// <summary>
        /// 设置初始数量
        /// </summary>
        /// <param name="initialCount"></param>
        public abstract void SetInitialCount(int initialCount);

        /// <summary>
        /// 设置对象池名称
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// 每帧后更新
        /// </summary>
        public abstract void LateTick();

        /// <summary>
        /// 获取当前数量
        /// </summary>
        /// <returns></returns>
        public abstract int GetCurrentCount();

        /// <summary>
        /// 获取对象类型
        /// </summary>
        /// <returns></returns>
        public abstract Type GetObjectType();

        /// <summary>
        /// 清空对象池
        /// </summary>
        public abstract void Clear();


        /// <summary>
        /// 对象池名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 初始数量
        /// </summary>
        public int InitialCount { get { return initialCount; } }

        /// <summary>
        /// Tick频率 每隔N帧Tick一次
        /// </summary>
        public int TickFrequency
        {
            get { return tickFrequency; }

            set { tickFrequency = value; }
        }

        /// <summary>
        /// 最大数量 大于最大值时，Tick时销毁一个
        /// </summary>
        public int MaxCount
        {
            get { return maxCount; }
            set { maxCount = value; }
        }

        /// <summary>
        /// 最大极限数量 大于最大极限值时，Tick时销毁至最大数量
        /// </summary>
        public int MaxLimitCount
        {
            get { return maxLimitCount; }
            set { maxLimitCount = value; }
        }

        /// <summary>
        /// 最小数量 小于最小值时，Tick时创建一个
        /// </summary>
        public int MinCount
        {
            get { return minCount; }
            set { minCount = value; }
        }

        /// <summary>
        /// 最小极限数量 小于最小极限值时，Tick时创建至最小数量
        /// </summary>
        public int MinLimitCount
        {
            get { return minLimitCount; }
            set { minLimitCount = value; }
        }

        /// <summary>
        /// 当前数量
        /// </summary>
        public int CurrentCount => GetCurrentCount();

        protected string name;
        protected int initialCount = 0;
        protected int tickFrequency = 0;
        protected int maxCount = int.MaxValue;
        protected int maxLimitCount = int.MaxValue;
        protected int minCount = 0;
        protected int minLimitCount = 0;
    }


    /// <summary>
    /// Object对象池抽象基类
    /// </summary>
    public abstract class ObjectPoolBase : PoolBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        public abstract void Initialize(string name);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        public abstract void Initialize(string name, int tickFrequency);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        public abstract void Initialize(string name, int tickFrequency, int maxCount, int minCount);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        public abstract void Initialize(string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        public abstract void Initialize(string name, int initialCount, int tickFrequency);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        public abstract void Initialize(string name, int initialCount, int tickFrequency, int maxCount, int minCount);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        public abstract void Initialize(string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount);
    }
}
