using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
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
        public static void RegisterController(Controller controller)
        {
            if (!Instance.m_controllers.Contains(controller))
            {
                Instance.m_controllers.Add(controller);
            }
            else
            {
                Debug.LogError($"controller {controller} is already registered.");
            }
        }

        /// <summary>
        /// 注销Controller
        /// </summary>
        /// <param name="controller"></param>
        public static void UnregisterController(Controller controller)
        {
            if (Instance.m_controllers.Contains(controller))
            {
                Instance.m_controllers.Remove(controller);
                ReferencePool.Release(controller);
            }
            else
            {
                Debug.LogError($"controller {controller} is not registered.");
            }
        }

        /// <summary>
        /// 初始化Controller管理器
        /// </summary>
        public static void Initialize()
        {
            foreach (var controller in Instance.m_controllers)
            {
                controller.Initialize();
            }
        }

        /// <summary>
        /// Controller管理器第一帧更新
        /// </summary>
        public static void OnFirstFrame()
        {
            foreach (var controller in Instance.m_controllers)
            {
                controller.OnFirstFrame();
            }
        }

        /// <summary>
        /// Controller管理器销毁
        /// </summary>
        public static void Uninitialize()
        {
            foreach (var controller in Instance.m_controllers)
            {
                controller.Clear();
            }
        }

        /// <summary>
        /// Controller管理器更新
        /// </summary>
        public static void TickControllerManager() => Instance.Tick();

        /// <summary>
        /// Controller管理器Late更新
        /// </summary>
        public static void LateTickControllerManager() => Instance.LateTick();
        #endregion


        #region 实现接口 ITickable
        public void Tick()
        {
            var controllers = Instance.m_controllers;
            var count = controllers.Count;
            for (int i = 0; i < count; i++)
            {
                controllers[i].Tick();
            }
        }

        public void LateTick()
        {
            var controllers = Instance.m_controllers;
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
        public static IReadOnlyList<Controller> Controllers => Instance.m_controllers;
        #endregion

        private static ControllerManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ControllerManager();
                }
                return s_instance;
            }
        }
        private static ControllerManager s_instance;

        private List<Controller> m_controllers = new List<Controller>();
    }
}
