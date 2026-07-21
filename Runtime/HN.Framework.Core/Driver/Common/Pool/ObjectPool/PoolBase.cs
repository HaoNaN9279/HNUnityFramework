using System;
using System.Collections;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Driver.Common.Pool.ObjectPool
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
            this._name = name;
        }

        /// <summary>
        /// 由 Tick 获取当前存储数量
        /// </summary>
        protected abstract int GetStoredCount();

        /// <summary>
        /// 由 Tick 在数量低于下限时创建一个对象
        /// </summary>
        protected abstract void TickSpawn();

        /// <summary>
        /// 由 Tick 在数量高于上限时销毁一个对象
        /// </summary>
        protected abstract void TickDespawn();

        /// <summary>
        /// 每帧更新
        /// </summary>
        public virtual void Tick()
        {
            if (PoolTickFrequency != 0 && HNLogicTime.LogicFrameCount % (ulong)PoolTickFrequency != 0)
                return;

            int count = GetStoredCount();
            if (count > PoolMaxCount && count < PoolMaxLimitCount)
                TickDespawn();
            else if (count > PoolMaxLimitCount)
                for (int i = 0; i < count - PoolMaxLimitCount; i++)
                    TickDespawn();

            count = GetStoredCount();
            if (count < PoolMinCount && count > PoolMinLimitCount)
                TickSpawn();
            else if (count < PoolMinLimitCount)
                for (int i = 0; i < PoolMinLimitCount - count; i++)
                    TickSpawn();
        }

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
        public string Name => _name;

        /// <summary>
        /// 初始数量
        /// </summary>
        public int InitialCount { get { return _initialCount; } }

        /// <summary>
        /// Tick频率 每隔N帧Tick一次
        /// </summary>
        public int TickFrequency
        {
            get { return _tickFrequency; }

            set { _tickFrequency = value; }
        }

        /// <summary>
        /// 最大数量 大于最大值时，Tick时销毁一个
        /// </summary>
        public int MaxCount
        {
            get { return _maxCount; }
            set { _maxCount = value; }
        }

        /// <summary>
        /// 最大极限数量 大于最大极限值时，Tick时销毁至最大数量
        /// </summary>
        public int MaxLimitCount
        {
            get { return _maxLimitCount; }
            set { _maxLimitCount = value; }
        }

        /// <summary>
        /// 最小数量 小于最小值时，Tick时创建一个
        /// </summary>
        public int MinCount
        {
            get { return _minCount; }
            set { _minCount = value; }
        }

        /// <summary>
        /// 最小极限数量 小于最小极限值时，Tick时创建至最小数量
        /// </summary>
        public int MinLimitCount
        {
            get { return _minLimitCount; }
            set { _minLimitCount = value; }
        }

        /// <summary>
        /// 当前数量
        /// </summary>
        public int CurrentCount => GetCurrentCount();

        private string _name;
        private int _initialCount = 0;
        private int _tickFrequency = 0;
        private int _maxCount = int.MaxValue;
        private int _maxLimitCount = int.MaxValue;
        private int _minCount = 0;
        private int _minLimitCount = 0;

        /// <summary>
        /// 对象池名称
        /// </summary>
        protected string PoolName { get => _name; set => _name = value; }

        /// <summary>
        /// 初始数量
        /// </summary>
        protected int PoolInitialCount { get => _initialCount; set => _initialCount = value; }

        /// <summary>
        /// Tick频率
        /// </summary>
        protected int PoolTickFrequency { get => _tickFrequency; set => _tickFrequency = value; }

        /// <summary>
        /// 最大数量
        /// </summary>
        protected int PoolMaxCount { get => _maxCount; set => _maxCount = value; }

        /// <summary>
        /// 最大极限数量
        /// </summary>
        protected int PoolMaxLimitCount { get => _maxLimitCount; set => _maxLimitCount = value; }

        /// <summary>
        /// 最小数量
        /// </summary>
        protected int PoolMinCount { get => _minCount; set => _minCount = value; }

        /// <summary>
        /// 最小极限数量
        /// </summary>
        protected int PoolMinLimitCount { get => _minLimitCount; set => _minLimitCount = value; }
    }


    /// <summary>
    /// Object对象池抽象基类
    /// </summary>
    public abstract class ObjectPoolBase : PoolBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="settings">对象池配置参数</param>
        public abstract void Initialize(PoolSettings settings);
    }
}
