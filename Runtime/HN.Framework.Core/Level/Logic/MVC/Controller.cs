using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Level.Logic
{
    #region Controller接口
    /// <summary>
    /// Controller接口
    /// </summary>
    public interface IController : ITickable, IReference
    {
        /// <summary>
        /// Controller初始化
        /// </summary>
        public void Initialize();

        /// <summary>
        /// Controller第一次帧更新
        /// </summary>
        public void OnFirstFrame();

        /// <summary>
        /// 添加ControllerUnit
        /// </summary>
        /// <param name="unit"></param>
        public void AddUnit(ControllerUnit unit);

        /// <summary>
        /// 移除ControllerUnit
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveUnit(ControllerUnit unit);
    }
    #endregion

    /// <summary>
    /// Controller抽象类
    /// </summary>
    public abstract class Controller : IController
    {
        #region 实现接口 IController
        /// <summary>
        /// Controller初始化
        /// </summary>
        public void Initialize()
        {
            foreach (var unit in controllerUnits)
            {
                unit.Initialize();
            }
        }

        /// <summary>
        /// Controller第一帧更新
        /// </summary>
        public void OnFirstFrame()
        {
            foreach (var unit in controllerUnits)
            {
                unit.OnFirstFrame();
            }
        }

        /// <summary>
        /// Controller每帧更新
        /// </summary>
        public void Tick()
        {
            var units = controllerUnits;
            for (int i = 0, len = units.Count; i < len; i++)
            {
                units[i].Tick();
            }
        }

        /// <summary>
        /// Controller每帧后更新
        /// </summary>
        public void LateTick()
        {
            var units = controllerUnits;
            for (int i = 0, len = units.Count; i < len; i++)
            {
                units[i].LateTick();
            }
        }

        /// <summary>
        /// 添加Controller单元
        /// </summary>
        /// <param name="unit"></param>
        public void AddUnit(ControllerUnit unit)
        {
            if (!controllerUnits.Contains(unit))
            {
                controllerUnits.Add(unit);
            }
        }

        /// <summary>
        /// 移除Controller单元
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveUnit(ControllerUnit unit)
        {
            if (controllerUnits.Contains(unit))
            {
                controllerUnits.Remove(unit);
                ReferencePool.Release(unit);
            }
        }

        /// <summary>
        /// Controller销毁
        /// </summary>
        public void Clear()
        {
            var units = controllerUnits;
            for (int i = 0, len = units.Count; i < len; i++)
            {
                units[i].Clear();
            }
            ReferencePool.Release(controllerUnits);
        }
        #endregion

        #region 对外属性
        /// <summary>
        /// Controller单元列表
        /// </summary>
        public IReadOnlyList<ControllerUnit> ControllerUnits => controllerUnits;
        #endregion

        #region 私有字段
        protected PooledList<ControllerUnit> controllerUnits = ReferencePool.Acquire<PooledList<ControllerUnit>>();
        #endregion
    }
}
