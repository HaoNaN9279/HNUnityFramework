using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

namespace HN.Framework
{
    #region 状态机管理器接口
    public interface IFSMManager : ITickable
    {
        /// <summary>
        /// 获取状态机
        /// </summary>
        /// <param name="name">状态机名称</param>
        /// <returns>状态机实例</returns>
        public IFSM Get(string name);

        /// <summary>
        /// 创建状态机
        /// </summary>
        /// <typeparam name="T">状态机类型</typeparam>
        /// <param name="name">状态机名称</param>
        /// <returns>创建的状态机实例</returns>
        public IFSM Create<T>(string name) where T : class, IFSM, new();

        /// <summary>
        /// 移除状态机
        /// </summary>
        /// <param name="name">状态机名称</param>
        public void Remove(string name);

        /// <summary>
        /// 移除状态机
        /// </summary>
        /// <param name="fsm">状态机实例</param>
        public void Remove(IFSM fsm);

        /// <summary>
        /// 清除所有状态机
        /// </summary>
        public void ClearAll();
    }
    #endregion

    /// <summary>
    /// 状态机管理器
    /// 依赖 ReferencePool
    /// 负责管理所有状态机的创建、获取、移除和更新
    /// </summary>
    public sealed class FSMManager : IFSMManager
    {
        #region 对外函数
        /// <summary>
        /// 初始化状态机管理器
        /// </summary>
        public static void Initialize()
        {

        }

        /// <summary>
        /// 反初始化状态机管理器
        /// </summary>
        public static void Uninitialize()
        {
            Instance.ClearAll();
        }

        /// <summary>
        /// 获取状态机
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFSM GetFSM(string name) => Instance.Get(name);

        /// <summary>
        /// 创建状态机
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFSM CreateFSM<T>(string name) where T : class, IFSM, new() => Instance.Create<T>(name);

        /// <summary>
        /// 移除状态机
        /// </summary>
        /// <param name="name"></param>
        public static void RemoveFSM(string name) => Instance.Remove(name);

        /// <summary>
        /// 移除状态机
        /// </summary>
        /// <param name="fsm"></param>
        public static void RemoveFSM(IFSM fsm) => Instance.Remove(fsm);

        /// <summary>
        /// 每帧更新状态机管理器
        /// </summary>
        public static void TickFSMManager() => Instance.Tick();

        /// <summary>
        /// 每帧后更新状态机管理器
        /// </summary>
        public static void LateTickFSMManager() => Instance.LateTick();
        #endregion

        #region 实现接口 ITickable
        /// <summary>
        /// 每帧更新状态机管理器
        /// </summary>
        public void Tick()
        {
            if (m_fsmDict.Count == 0)
            {
                return;
            }

            foreach (var fsm in m_fsmDict.Values)
            {
                if (fsm.Paused)
                {
                    continue;
                }
                fsm.Tick();
            }
        }

        /// <summary>
        /// 每帧后更新状态机管理器
        /// </summary>
        public void LateTick()
        {
            if (m_fsmDict.Count == 0)
            {
                return;
            }

            foreach (var fsm in m_fsmDict.Values)
            {
                if (fsm.Paused)
                {
                    continue;
                }
                fsm.LateTick();
            }
        }
        #endregion

        #region 实现接口 IFSMManager
        public IFSM Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("FSM name is null or empty.");
                return null;
            }

            if (!m_fsmDict.ContainsKey(name))
            {
                Debug.LogError($"FSM with name {name} does not exist.");
                return null;
            }

            return m_fsmDict[name];
        }

        public IFSM Create<T>(string name) where T : class, IFSM, new()
        {
            if (m_fsmDict.ContainsKey(name))
            {
                Debug.LogError($"fsm name {name} already contained.");
                return null;
            }

            IFSM fsm = ReferencePool.Acquire<T>();
            fsm.Initialize(name);
            m_fsmDict.Add(name, fsm);
            return fsm;
        }

        public void Remove(string name)
        {
            if (!m_fsmDict.ContainsKey(name))
            {
                Debug.LogError($"fsm {name} does not exist.");
                return;
            }

            IFSM fsm = m_fsmDict[name];
            m_fsmDict.Remove(name);
            ReferencePool.Release(fsm);
        }

        public void Remove(IFSM fsm)
        {
            if (!m_fsmDict.ContainsValue(fsm))
            {
                Debug.LogError($"fsm {fsm.Name} does not exist.");
                return;
            }

            m_fsmDict.Remove(fsm.Name);
            ReferencePool.Release(fsm);
        }

        public void ClearAll()
        {
            if (m_fsmDict.Count == 0)
            {
                return;
            }

            foreach (var fsm in m_fsmDict.Values)
            {
                ReferencePool.Release(fsm);
            }
            m_fsmDict.Clear();
        }
        #endregion

        #region 对外属性
        #endregion

        #region 私有变量
        private static FSMManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new FSMManager();
                }
                return m_instance;
            }
        }
        private static FSMManager m_instance;
        private static Dictionary<string, IFSM> m_fsmDict = new Dictionary<string, IFSM>();
        #endregion
    }
}
