using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Capability
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
        /// 通过类型改变Procedure状态
        /// </summary>
        /// <typeparam name="T">ProcedureState 类型</typeparam>
        public void ChangeState<T>() where T : ProcedureState;

        /// <summary>
        /// 当前Procedure状态名称（未启动时为 null）
        /// </summary>
        public string CurrentStateName { get; }

        /// <summary>
        /// 清除所有Procedure状态
        /// </summary>
        public void ClearAll();
    }
    #endregion

    public sealed class ProcedureManager : IProcedureManager
    {
        #region 对外函数
        public void Initialize()
        {

        }

        public void Uninitialize()
        {
            ClearAll();
        }
        #endregion

        #region 实现接口 ITickable
        /// <summary>
        /// 每帧更新Procedure管理器
        /// </summary>
        public void Tick()
        {
            if (m_currentState != null)
            {
                try
                {
                    m_currentState.InvokeTickEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.Tick] Exception: {e}");
                }
            }
        }

        /// <summary>
        /// 每帧后更新Procedure管理器
        /// </summary>
        public void LateTick()
        {
            if (m_currentState != null)
            {
                try
                {
                    m_currentState.InvokeLateTickEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.LateTick] Exception: {e}");
                }
            }
        }
        #endregion

        #region 实现接口 IProcedureManager
        public void Start(string stateName)
        {
            if (m_states.ContainsKey(stateName))
            {
                m_currentState = m_states[stateName];
                try
                {
                    m_currentState.InvokeEnterEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.Start({stateName})] Exception: {e}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Current procedure does not contain state {stateName}.");
            }
        }

        public void Start(ProcedureState startState)
        {
            if (m_states.ContainsValue(startState))
            {
                m_currentState = startState;
                try
                {
                    m_currentState.InvokeEnterEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.Start({startState.Name})] Exception: {e}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Current procedure does not contain state {startState}.");
            }
        }

        public void Shutdown()
        {
            if (m_currentState != null)
            {
                m_currentState.CancelAllSubProcesses();
                try
                {
                    m_currentState.InvokeExitEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.Shutdown] ExitEvent exception: {e}");
                }
            }
            m_currentState = null;
        }

        public T AddState<T>(string stateName) where T : ProcedureState, new()
        {
            if (m_states.ContainsKey(stateName))
            {
                throw new InvalidOperationException($"Already contains procedure state name {stateName}.");
            }
            T newState = ReferencePool.Acquire<T>();
            newState.Initialize(stateName);
            m_states[stateName] = newState;
            return newState;
        }

        public void AddState(ProcedureState state)
        {
            if (m_states.ContainsKey(state.Name))
            {
                throw new InvalidOperationException($"Already contains procedure state name {state.Name}.");
            }

            m_states[state.Name] = state;
        }

        public void RemoveState(string stateName)
        {
            if (!m_states.ContainsKey(stateName))
            {
                throw new InvalidOperationException($"state {stateName} does not exist.");
            }

            ProcedureState state = m_states[stateName];
            m_states.Remove(stateName);
            ReferencePool.Release(state);
        }

        public void RemoveState(ProcedureState state)
        {
            if (!m_states.ContainsValue(state))
            {
                throw new InvalidOperationException($"state {state} not exist.");
            }

            m_states.Remove(state.Name);
            ReferencePool.Release(state);
        }

        public void ChangeState(string targetStateName)
        {
            if (m_isChanging) return;

            if (m_currentState == null)
            {
                throw new InvalidOperationException("Procedure has not started.");
            }

            if (m_currentState.Name == targetStateName) return;

            if (!m_states.ContainsKey(targetStateName))
            {
                throw new InvalidOperationException($"Procedure does not contain state {targetStateName}.");
            }

            m_isChanging = true;
            try
            {
                m_currentState?.CancelAllSubProcesses();
                try
                {
                    m_currentState.InvokeExitEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.ChangeState({targetStateName})] ExitEvent exception: {e}");
                }
                m_currentState = m_states[targetStateName];
                try
                {
                    m_currentState.InvokeEnterEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.ChangeState({targetStateName})] EnterEvent exception: {e}");
                }
            }
            finally
            {
                m_isChanging = false;
            }
        }

        public void ChangeState(ProcedureState targetState)
        {
            if (m_isChanging) return;

            if (m_currentState == null)
            {
                throw new InvalidOperationException("Procedure has not started.");
            }

            if (m_currentState == targetState) return;

            if (!m_states.ContainsValue(targetState))
            {
                throw new InvalidOperationException($"Procedure does not contain state {targetState}.");
            }

            m_isChanging = true;
            try
            {
                m_currentState?.CancelAllSubProcesses();
                try
                {
                    m_currentState.InvokeExitEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.ChangeState({targetState.Name})] ExitEvent exception: {e}");
                }
                m_currentState = targetState;
                try
                {
                    m_currentState.InvokeEnterEvent();
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcedureManager.ChangeState({targetState.Name})] EnterEvent exception: {e}");
                }
            }
            finally
            {
                m_isChanging = false;
            }
        }

        /// <summary>
        /// 通过类型改变Procedure状态
        /// </summary>
        /// <typeparam name="T">目标ProcedureState类型</typeparam>
        public void ChangeState<T>() where T : ProcedureState
        {
            if (m_currentState == null)
            {
                throw new InvalidOperationException("Procedure has not started.");
            }

            foreach (var pair in m_states)
            {
                if (pair.Value is T)
                {
                    ChangeState(pair.Key);
                    return;
                }
            }

            throw new InvalidOperationException($"Procedure does not contain state of type {typeof(T).FullName}.");
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
        public int ProcedureStateCount => m_states.Count;

        /// <summary>
        /// 当前Procedure状态
        /// </summary>
        public ProcedureState CurrentState => m_currentState;

        /// <summary>
        /// 当前Procedure状态名称（未启动时为 null）
        /// </summary>
        public string CurrentStateName => m_currentState?.Name;
        #endregion

        #region 私有变量
        private Dictionary<string, ProcedureState> m_states = new Dictionary<string, ProcedureState>();
        private ProcedureState m_currentState;
        private bool m_isChanging = false;
        #endregion
    }
}
