using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic
{
    /// <summary>
    /// Controller管理器
    /// </summary>
    public sealed class ControllerManager : ITickable
    {
        #region 对外函数
        /// <summary>
        /// 注册Controller
        /// </summary>
        /// <param name="controller"></param>
        public void RegisterController(Controller controller)
        {
            if (!m_controllers.Contains(controller))
            {
                m_controllers.Add(controller);
            }
            else
            {
                throw new InvalidOperationException($"controller {controller} is already registered.");
            }
        }

        /// <summary>
        /// 注销Controller
        /// </summary>
        /// <param name="controller"></param>
        public void UnregisterController(Controller controller)
        {
            if (m_controllers.Contains(controller))
            {
                m_controllers.Remove(controller);
                ReferencePool.Release(controller);
            }
            else
            {
                throw new InvalidOperationException($"controller {controller} is not registered.");
            }
        }

        /// <summary>
        /// 初始化Controller管理器
        /// </summary>
        public void Initialize()
        {
            foreach (var controller in m_controllers)
            {
                controller.Initialize();
            }
        }

        /// <summary>
        /// Controller管理器第一帧更新
        /// </summary>
        public void OnFirstFrame()
        {
            foreach (var controller in m_controllers)
            {
                controller.OnFirstFrame();
            }
        }

        /// <summary>
        /// Controller管理器销毁
        /// </summary>
        public void Uninitialize()
        {
            foreach (var controller in m_controllers)
            {
                controller.Clear();
            }
        }
        #endregion


        #region 实现接口 ITickable
        public void Tick()
        {
            var controllers = m_controllers;
            var count = controllers.Count;
            for (int i = 0; i < count; i++)
            {
                controllers[i].Tick();
            }
        }

        public void LateTick()
        {
            var controllers = m_controllers;
            var count = controllers.Count;
            for (int i = 0; i < count; i++)
            {
                controllers[i].LateTick();
            }
        }
        #endregion


        #region 对外属性
        /// <summary>
        /// 所有Controller的列表
        /// </summary>
        public IReadOnlyList<Controller> Controllers => m_controllers;
        #endregion

        private List<Controller> m_controllers = new List<Controller>();
    }
}
