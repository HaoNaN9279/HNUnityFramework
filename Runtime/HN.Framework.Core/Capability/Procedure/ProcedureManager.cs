using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

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
                m_currentState.InvokeTickEvent();
            }
        }

        /// <summary>
        /// 每帧后更新Procedure管理器
        /// </summary>
        public void LateTick()
        {
            if (m_currentState != null)
            {
                m_currentState.InvokeLateTickEvent();
            }
        }
        #endregion

        #region 实现接口 IProcedureManager
        public void Start(string stateName)
        {
            if (m_states.ContainsKey(stateName))
            {
                m_currentState = m_states[stateName];
                m_currentState.InvokeEnterEvent();
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
                m_currentState.InvokeEnterEvent();
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
                m_currentState.InvokeExitEvent();
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
            if (m_states.ContainsValue(state))
            {
                throw new InvalidOperationException($"Already contains procedure state {state}.");
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
            if (m_currentState == null)
            {
                throw new InvalidOperationException("Procedure has not start.");
            }

            if (!m_states.ContainsKey(targetStateName))
            {
                throw new InvalidOperationException($"Procedure does not contain state {targetStateName}.");
            }

            m_currentState.InvokeExitEvent();
            m_currentState = m_states[targetStateName];
            m_currentState.InvokeEnterEvent();
        }

        public void ChangeState(ProcedureState targetState)
        {
            if (m_currentState == null)
            {
                throw new InvalidOperationException("Procedure has not start.");
            }

            if (!m_states.ContainsValue(targetState))
            {
                throw new InvalidOperationException($"Procedure does not contain state {targetState}.");
            }

            m_currentState.InvokeExitEvent();
            m_currentState = targetState;
            m_currentState.InvokeEnterEvent();
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
        #endregion

        #region 私有变量
        private Dictionary<string, ProcedureState> m_states = new Dictionary<string, ProcedureState>();
        private ProcedureState m_currentState;
        #endregion
    }
}
