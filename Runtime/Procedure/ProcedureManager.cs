using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    #region Procedure管理器接口
    /// <summary>
    /// Procedure管理器接口
    /// </summary>
    public interface IProcedureManager : ITickable
    {
        /// <summary>
        /// 启动Procedure管理器
        /// </summary>
        /// <param name="stateName"></param>
        public void Start(string stateName);

        /// <summary>
        /// 启动Procedure管理器
        /// </summary>
        /// <param name="startState"></param>
        public void Start(ProcedureState startState);

        /// <summary>
        /// 关闭Procedure管理器
        /// </summary>
        public void Shutdown();

        /// <summary>
        /// 添加Procedure状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public T AddState<T>(string stateName) where T : ProcedureState, new();

        /// <summary>
        /// 添加Procedure状态
        /// </summary>
        /// <param name="state"></param>
        public void AddState(ProcedureState state);

        /// <summary>
        /// 移除Procedure状态
        /// </summary>
        /// <param name="stateName"></param>
        public void RemoveState(string stateName);

        /// <summary>
        /// 移除Procedure状态
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(ProcedureState state);

        /// <summary>
        /// 改变Procedure状态
        /// </summary>
        /// <param name="targetStateName"></param>
        public void ChangeState(string targetStateName);

        /// <summary>
        /// 改变Procedure状态
        /// </summary>
        /// <param name="targetState"></param>
        public void ChangeState(ProcedureState targetState);

        /// <summary>
        /// 清除所有Procedure状态
        /// </summary>
        public void ClearAll();
    }
    #endregion

    public sealed class ProcedureManager : IProcedureManager
    {
        #region 对外函数
        public static void Initialize()
        {

        }

        public static void Uninitialize()
        {
            Instance.ClearAll();
        }

        /// <summary>
        /// 启动Procedure管理器
        /// </summary>
        /// <param name="stateName"></param>
        public static void StartProcedure(string stateName) => Instance.Start(stateName);

        /// <summary>
        /// 启动Procedure管理器
        /// </summary>
        /// <param name="startState"></param>
        public static void StartProcedure(ProcedureState startState) => Instance.Start(startState);

        /// <summary>
        /// 关闭Procedure管理器
        /// </summary>
        public static void ShutdownProcedure() => Instance.Shutdown();

        /// <summary>
        /// 添加Procedure状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public static T AddProcedureState<T>(string stateName) where T : ProcedureState, new() => Instance.AddState<T>(stateName);

        /// <summary>
        /// 添加Procedure状态
        /// </summary>
        /// <param name="state"></param>
        public static void AddProcedureState(ProcedureState state) => Instance.AddState(state);

        /// <summary>
        /// 移除Procedure状态
        /// </summary>
        /// <param name="stateName"></param>
        public static void RemoveProcedureState(string stateName) => Instance.RemoveState(stateName);

        /// <summary>
        /// 移除Procedure状态
        /// </summary>
        /// <param name="state"></param>
        public static void RemoveProcedureState(ProcedureState state) => Instance.RemoveState(state);

        /// <summary>
        /// 改变Procedure状态
        /// </summary>
        /// <param name="targetStateName"></param>
        public static void ChangeProcedureState(string targetStateName) => Instance.ChangeState(targetStateName);

        /// <summary>
        /// 改变Procedure状态
        /// </summary>
        /// <param name="targetState"></param>
        public static void ChangeProcedureState(ProcedureState targetState) => Instance.ChangeState(targetState);

        /// <summary>
        /// 每帧更新Procedure管理器
        /// </summary>
        public static void TickProcedureManager() => Instance.Tick();

        /// <summary>
        /// 每帧后更新Procedure管理器
        /// </summary>
        public static void LateTickProcedureManager() => Instance.LateTick();
        #endregion

        #region 实现接口 ITickable
        /// <summary>
        /// 每帧更新Procedure管理器
        /// </summary>
        public void Tick()
        {
            if (m_currentState != null)
            {
                m_currentState.TickEvent?.Invoke();
            }
        }

        /// <summary>
        /// 每帧后更新Procedure管理器
        /// </summary>
        public void LateTick()
        {
            if (m_currentState != null)
            {
                m_currentState.LateTickEvent?.Invoke();
            }
        }
        #endregion

        #region 实现接口 IProcedureManager
        public void Start(string stateName)
        {
            if (m_states.ContainsKey(stateName))
            {
                m_currentState = m_states[stateName];
                m_currentState.EnterEvent?.Invoke();
            }
            else
            {
                Debug.LogError($"Current procedure does not contain state {stateName}.");
            }
        }

        public void Start(ProcedureState startState)
        {
            if (m_states.ContainsValue(startState))
            {
                m_currentState = startState;
                m_currentState.EnterEvent?.Invoke();
            }
            else
            {
                Debug.LogError($"Current procedure does not contain state {startState}.");
            }
        }

        public void Shutdown()
        {
            if (m_currentState != null)
            {
                m_currentState.ExitEvent?.Invoke();
            }
            m_currentState = null;
        }

        public T AddState<T>(string stateName) where T : ProcedureState, new()
        {
            if (m_states.ContainsKey(stateName))
            {
                Debug.LogError($"Already contains procedure state name {stateName}.");
                return null;
            }
            T newState = ReferencePool.Acquire<T>();
            newState.Initialize(stateName);
            m_states[stateName] = newState;
            return newState;
        }

        public void AddState(ProcedureState state)
        {
            if (m_states.ContainsValue(state))
            {
                Debug.LogError($"Already contains procedure state {state}.");
                return;
            }

            m_states[state.Name] = state;
        }

        public void RemoveState(string stateName)
        {
            if (!m_states.ContainsKey(stateName))
            {
                Debug.LogError($"state {stateName} does not exist.");
                return;
            }

            ProcedureState state = m_states[stateName];
            m_states.Remove(stateName);
            ReferencePool.Release(state);
        }
        public void RemoveState(ProcedureState state)
        {
            if (!m_states.ContainsValue(state))
            {
                Debug.LogError($"state {state} not exist.");
                return;
            }

            m_states.Remove(state.Name);
            ReferencePool.Release(state);
        }

        public void ChangeState(string targetStateName)
        {
            if (m_currentState == null)
            {
                Debug.LogError($"Procedure has not start.");
            }

            if (!m_states.ContainsKey(targetStateName))
            {
                Debug.LogError($"Procedure does not contain state {targetStateName}.");
            }

            m_currentState.ExitEvent?.Invoke();
            m_currentState = m_states[targetStateName];
            m_currentState.EnterEvent?.Invoke();
        }

        public void ChangeState(ProcedureState targetState)
        {
            if (m_currentState == null)
            {
                Debug.LogError($"Procedure has not start.");
            }

            if (!m_states.ContainsValue(targetState))
            {
                Debug.LogError($"Procedure does not contain state {targetState}.");
            }

            m_currentState.ExitEvent?.Invoke();
            m_currentState = targetState;
            m_currentState.EnterEvent?.Invoke();
        }

        /// <summary>
        /// 清理Procedure管理器
        /// </summary>
        public void ClearAll()
        {
            foreach (var state in m_states)
            {
                ReferencePool.Release(state.Value);
            }
            m_states.Clear();
            m_currentState = null;
        }
        #endregion


        #region 对外属性
        /// <summary>
        /// Procedure状态数量
        /// </summary>
        public static int ProcedureStateCount => Instance.m_states.Count;

        /// <summary>
        /// 当前Procedure状态
        /// </summary>
        public static ProcedureState CurrentState => Instance.m_currentState;
        #endregion

        #region 私有变量
        private static ProcedureManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ProcedureManager();
                    return m_instance;
                }

                return m_instance;
            }
        }
        private static ProcedureManager m_instance;
        private Dictionary<string, ProcedureState> m_states = new Dictionary<string, ProcedureState>();
        private ProcedureState m_currentState;
        #endregion
    }
}
