using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic
{
    #region Model接口
    /// <summary>
    /// Model接口
    /// </summary>
    public interface IModel : ITickable
    {
        /// <summary>
        /// Model初始化
        /// </summary>
        public void Initialize();

        /// <summary>
        /// Model第一帧更新
        /// </summary>
        public void OnFirstFrame();

        /// <summary>
        /// 添加Model单元
        /// </summary>
        /// <param name="unit"></param>
        public void AddUnit(ModelUnit unit);

        /// <summary>
        /// 移除Model单元
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveUnit(ModelUnit unit);
    }
    #endregion

    public abstract class Model : IModel
    {
        #region 实现接口 IModel
        /// <summary>
        /// Model初始化
        /// </summary>
        public void Initialize()
        {
            int count = modelUnits.Count;
            for (int i = 0; i < count; i++)
            {
                modelUnits[i].Initialize();
            }
            OnInitialize();
        }

        /// <summary>
        /// Model第一帧更新
        /// </summary>
        public void OnFirstFrame()
        {
            int count = modelUnits.Count;
            for (int i = 0; i < count; i++)
            {
                modelUnits[i].OnFirstFrame();
            }
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick()
        {
            int count = modelUnits.Count;
            for (int i = 0; i < count; i++)
            {
                modelUnits[i].Tick();
            }
        }

        /// <summary>
        /// 每帧后更新
        /// </summary>
        public void LateTick()
        {
            int count = modelUnits.Count;
            for (int i = 0; i < count; i++)
            {
                modelUnits[i].LateTick();
            }
        }

        /// <summary>
        /// 添加Model单元
        /// </summary>
        /// <param name="unit"></param>
        public void AddUnit(ModelUnit unit)
        {
            if (!modelUnits.Contains(unit))
            {
                modelUnits.Add(unit);
            }
        }

        /// <summary>
        /// 移除Model单元
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveUnit(ModelUnit unit)
        {
            if (modelUnits.Contains(unit))
            {
                modelUnits.Remove(unit);
                ReferencePool.Release(unit);
            }
        }

        /// <summary>
        /// Model销毁
        /// </summary>
        public void Clear()
        {
            OnClear();
            int count = modelUnits.Count;
            for (int i = 0; i < count; i++)
            {
                modelUnits[i].Clear();
            }
            ReferencePool.Release(modelUnits);
        }
        #endregion

        #region 虚方法
        /// <summary>
        /// 初始化完成时调用，子类可重写以执行自定义初始化逻辑
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 销毁时调用，子类可重写以执行自定义清理逻辑
        /// </summary>
        protected virtual void OnClear() { }
        #endregion

        #region 对外属性
        /// <summary>
        /// Model单元列表
        /// </summary>
        public IReadOnlyList<ModelUnit> ModelUnits => modelUnits;
        #endregion

        #region 私有字段
        protected PooledList<ModelUnit> modelUnits = ReferencePool.Acquire<PooledList<ModelUnit>>();
        #endregion
    }
}
