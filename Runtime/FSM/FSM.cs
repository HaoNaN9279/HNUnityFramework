using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    #region 状态机接口
    public interface IFSM : IReference, ITickable
    {
        /// <summary>
        /// 状态机名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 状态机状态数量
        /// </summary>
        public int StateCount { get; }

        /// <summary>
        /// 是否暂停状态机
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// 状态机当前状态
        /// </summary>
        public IFSMState CurrentState { get; }

        /// <summary>
        /// 初始化状态机
        /// </summary>
        /// <param name="name">状态机名称</param>
        public void Initialize(string name);

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <param name="stateName"></param>
        public void Start(string stateName);

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <param name="startState"></param>
        public void Start(FSMState startState);

        /// <summary>
        /// 关闭状态机
        /// </summary>
        public void Shutdown();

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public T AddState<T>(string stateName) where T : FSMState, new();

        /// <summary>
        /// 添加状态转换
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public void AddTransition(FSMState fromState, FSMState targetState, Func<bool> condition);

        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="stateName"></param>
        public void RemoveState(string stateName);

        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(FSMState state);

        /// <summary>
        /// 移除状态转换
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        public void RemoveTransition(FSMState fromState, FSMState targetState);

        /// <summary>
        /// 改变状态机状态
        /// </summary>
        /// <param name="targetStateName"></param>
        public void ChangeState(string targetStateName);

        /// <summary>
        /// 改变状态机状态
        /// </summary>
        /// <param name="targetState"></param>
        public void ChangeState(FSMState targetState);
    }
    #endregion

    public class FSM : IFSM
    {
        #region 对外函数
        public FSM()
        {
        }

        /// <summary>
        /// 初始化状态机
        /// </summary>
        /// <param name="name"></param>
        public virtual void Initialize(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <param name="stateName"></param>
        public void Start(string stateName)
        {
            if (states.ContainsKey(stateName))
            {
                currentState = states[stateName];
                currentState.EnterEvent?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contain state {stateName}.");
            }
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <param name="startState"></param>
        public void Start(FSMState startState)
        {
            if (states.ContainsValue(startState))
            {
                currentState = startState;
                currentState.EnterEvent?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contain state {startState}.");
            }
        }

        /// <summary>
        /// 关闭状态机
        /// </summary>
        public void Shutdown()
        {
            if (currentState != null)
            {
                currentState.ExitEvent?.Invoke();
            }
            currentState = null;
        }

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public T AddState<T>(string stateName) where T : FSMState, new()
        {
            if (states.ContainsKey(stateName))
            {
                Debug.LogError($"Already contains state name {stateName}.");
                return null;
            }

            T newState = ReferencePool.Acquire<T>();
            newState.Initialize(stateName);
            states.Add(stateName, newState);
            newState.CreateEvent?.Invoke();
            return newState;
        }

        /// <summary>
        /// 添加状态转换
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        /// <param name="condition"></param>
        public virtual void AddTransition(FSMState fromState, FSMState targetState, Func<bool> condition)
        {
            if (!states.ContainsValue(fromState))
            {
                Debug.LogError($"FSM {this} does not exist state {fromState}.");
            }

            if (!states.ContainsValue(targetState))
            {
                Debug.LogError($"FSM {this} does not exist state {targetState}.");
            }

            fromState.AddTransition(targetState, condition);
        }

        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="stateName"></param>
        public void RemoveState(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                Debug.LogError($"state {stateName} does not exist.");
                return;
            }

            FSMState state = states[stateName];
            state.DestroyEvent?.Invoke();
            states.Remove(stateName);
            ReferencePool.Release(state);
        }

        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(FSMState state)
        {
            if (!states.ContainsValue(state))
            {
                Debug.LogError($"state {state} not exist.");
                return;
            }

            state.DestroyEvent?.Invoke();
            states.Remove(state.Name);
            ReferencePool.Release(state);
        }

        /// <summary>
        /// 移除状态转换
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="targetState"></param>
        public void RemoveTransition(FSMState fromState, FSMState targetState)
        {
            if (!states.ContainsValue(fromState))
            {
                Debug.LogError($"FSM {this} does not exist state {fromState}.");
            }

            if (!states.ContainsValue(targetState))
            {
                Debug.LogError($"FSM {this} does not exist state {targetState}.");
            }

            if (fromState.CanTransTo(targetState))
            {
                Debug.LogError($"FSM {this} state {fromState} can not trans to state {targetState}.");
            }

            fromState.RemoveTransition(targetState);
        }

        /// <summary>
        /// 改变状态机状态
        /// </summary>
        /// <param name="targetStateName"></param>
        public void ChangeState(string targetStateName)
        {
            if (states.ContainsKey(targetStateName))
            {
                currentState.ExitEvent?.Invoke();
                currentState = states[targetStateName];
                currentState.EnterEvent?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contains state {targetStateName}.");
            }
        }

        /// <summary>
        /// 改变状态机状态
        /// </summary>
        /// <param name="targetState"></param>
        public void ChangeState(FSMState targetState)
        {
            if (states.ContainsValue(targetState))
            {
                currentState.ExitEvent?.Invoke();
                currentState = targetState;
                currentState.EnterEvent?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contains state {targetState.Name}.");
            }
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick()
        {
            if (currentState != null)
            {
                foreach (var targetState in currentState.FSMTransitions.Keys)
                {
                    if (currentState.FSMTransitions[targetState].Invoke())
                    {
                        // Debug.Log($"Change State from {currentState.Name} to {targetState.Name}.");
                        ChangeState(targetState);
                        break;
                    }
                }

                currentState.UpdateEvent?.Invoke();
            }
        }

        /// <summary>
        /// 每帧后更新
        /// </summary>
        public void LateTick()
        {

        }

        /// <summary>
        /// 清理状态机
        /// </summary>
        public void Clear()
        {
            currentState = null;
            foreach (var state in states.Values)
            {
                ReferencePool.Release(state);
            }
            states.Clear();
            name = null;
        }
        #endregion

        #region 对外属性
        /// <summary>
        /// 状态机名称
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 状态机状态数量
        /// </summary>
        public int StateCount => states.Count;

        /// <summary>
        /// 是否暂停状态机
        /// </summary>
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }

        /// <summary>
        /// 状态机当前状态
        /// </summary>
        public IFSMState CurrentState => currentState;
        #endregion

        #region 内部变量
        /// <summary>
        /// 状态机名称
        /// </summary>
        protected string name;

        /// <summary>
        /// 状态机当前状态
        /// </summary>
        protected FSMState currentState;

        /// <summary>
        /// 是否暂停状态机
        /// </summary>
        protected bool paused = false;
        
        /// <summary>
        /// 状态机状态字典
        /// </summary>
        protected Dictionary<string, FSMState> states = new Dictionary<string, FSMState>();
        #endregion
    }
}
